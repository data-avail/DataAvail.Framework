using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;
using System.Reflection;
using System.Web;
using Microsoft.Data.Services.Toolkit.QueryModel;
using System.Linq.Expressions;
using System.Data.Services.Common;
using DataAvail.LinqMapper;
using DataAvail.Utils;
using System.Data;
using System.Data.Objects.DataClasses;

namespace DataAvail.DataService.Provider
{
    public abstract class Repository<E, T> : QRepository<E,T>, IRepository, IRepositoryEntity
        where E : class
        where T : class, new()
    {
        public Repository()
        {
        }

        protected ObjectContext Context { get; private set; }

        public void SetContext(object Context)
        {
            this.Context = (ObjectContext)Context;
        }

        private static Type GetMostBaseEntityType<ET>()
        {
            /*
            var baseType = typeof(ET);

            while (baseType.BaseType != typeof(object) && baseType.BaseType != typeof(EntityObject))
            {
                baseType = baseType.BaseType;
            }

            return baseType;
             */
            return GetMostBaseEntityType(typeof(ET));
        }

        private static Type GetMostBaseEntityType(Type ET)
        {
            var baseType = ET;

            while (baseType.BaseType != typeof(object) && baseType.BaseType != typeof(EntityObject))
            {
                baseType = baseType.BaseType;
            }

            return baseType;
        }

        protected override IQueryable<E> Queryable
        {
            get
            {
                var baseType = GetMostBaseEntityType<E>();

                IQueryable q = null;

                //searching for property on EntitySet 
                var pi = Context.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.PropertyType.Name == "ObjectSet`1" && p.PropertyType.GetGenericArguments()[0] == baseType && p.CanRead && !p.CanWrite).SingleOrDefault();

                if (pi != null)
                {
                    q = (IQueryable)pi.GetValue(Context, null);
                }
                else
                { 
                    //searching for extension method
                    var mi = DataAvail.Utils.Reflection.GetExtensionMethods(Context.GetType().Assembly, Context.GetType())
                        .Where(p => 
                            p.GetParameters().Length == 1 && 
                            p.GetParameters().First().ParameterType == Context.GetType() &&
                            p.ReturnType.Name == "IQueryable`1" && p.ReturnType.GetGenericArguments()[0] == baseType).SingleOrDefault();

                    if (mi == null)
                        throw new InvalidOperationException("Can't find related property or method on context for IQueryable");

                    q = (IQueryable)mi.Invoke(null, new object[] { Context });
                }

                

                if (baseType != typeof(E))
                {
                    q = q.OfType<E>();
                }

                return (IQueryable<E>)q;
            }
        }

        protected static int GetFieldValue<TT>(TT Item, string FieldName)
        {
            return (int)Item.GetType().GetProperty(FieldName).GetValue(Item, null);
        }

        private void SetKeyFieldValue(T Item, object Val)
        {
            Item.GetType().GetProperty(KeyFieldName).SetValue(Item, Val, null);
        }

        public object CreateDefaultEntity()
        {
            var item = new T();

            SetKeyFieldValue(item, -1);

            return item;

        }

        public void Save(object Item)
        {
            OnSave((T)Item);
        }

        protected virtual void OnSave(T Item)
        { 
            Save<E, T>(Item, KeyFieldName);
        }

        private static EntityKey GetEntityKey(object Entity)
        {
            var baseType = GetMostBaseEntityType(Entity.GetType());

            var keyProps = baseType.GetProperties()
                .Where(s => s.GetCustomAttributes(true)
                    .Where(p => p is EdmScalarPropertyAttribute && ((EdmScalarPropertyAttribute)p).EntityKeyProperty).SingleOrDefault() != null);

            var keys = keyProps.Select(p => new KeyValuePair<string, object>(p.Name, p.GetValue(Entity, null)));

            return new System.Data.EntityKey(string.Format("Model.{0}s", baseType.Name), keys);
        }

        protected void Save<ET, TT>(TT Item, string KeyFieldName)
        {
            var keyVal = GetFieldValue(Item, KeyFieldName);

            var entity = AutoMapper.Mapper.Map<TT, ET>(Item);

            if (keyVal < 0)
            {
                var mostBaseType = GetMostBaseEntityType<ET>();

                Context.AddObject(mostBaseType.Name + "s", entity);

                Context.SaveChanges();//(SaveOptions.DetectChangesBeforeSave);

                //Context.ObjectStateManager.ChangeObjectState(entity, System.Data.EntityState.Added);
                
            }
            else
            {
                var modelEntity = (ET)Context.GetObjectByKey(GetEntityKey(entity));

                var modelType = modelEntity.GetType();

                foreach (var propertyMap in AutoMapper.Mapper.FindTypeMapFor<TT, ET>().GetPropertyMaps().Where(p => Reflection.IsPrimitive(p.DestinationProperty.MemberType)))
                {
                    if (!propertyMap.IsIgnored())
                    {
                        var propertyInfo = modelType.GetProperty(propertyMap.DestinationProperty.Name);

                        propertyInfo.SetValue(modelEntity, propertyInfo.GetValue(entity, null), null);

                        Context.ObjectStateManager.GetObjectStateEntry(modelEntity).SetModifiedProperty(propertyMap.DestinationProperty.Name);
                    }
                }

                Context.SaveChanges(SaveOptions.DetectChangesBeforeSave);
            }

            //Context.SaveChanges(SaveOptions.DetectChangesBeforeSave);

            foreach (var propertyMap in AutoMapper.Mapper.FindTypeMapFor<ET, TT>().GetPropertyMaps().Where(p => Reflection.IsPrimitive(p.DestinationProperty.MemberType)))
            {
                if (!propertyMap.IsIgnored())
                {
                    var propertyInfo = typeof(TT).GetProperty(propertyMap.DestinationProperty.Name);

                    propertyInfo.SetValue(Item, ((PropertyInfo)propertyMap.SourceMember).GetValue(entity, null), null);
                }
            }
        }

        protected void UpdateEntityProperties<ET, TT>(ET Entity, TT Item)
        {
            var modelType = Entity.GetType();

            foreach (var propertyMap in AutoMapper.Mapper.FindTypeMapFor<TT, ET>().GetPropertyMaps().Where(p => Reflection.IsPrimitive(p.DestinationProperty.MemberType)))
            {
                if (!propertyMap.IsIgnored())
                {
                    var propertyInfo = modelType.GetProperty(propertyMap.DestinationProperty.Name);

                    if (propertyInfo.Name != KeyFieldName)
                    {
                        propertyInfo.SetValue(Entity, propertyInfo.GetValue(Entity, null), null);

                        Context.ObjectStateManager.GetObjectStateEntry(Entity).SetModifiedProperty(propertyMap.DestinationProperty.Name);
                    }
                }
            }       
        }

        public void Remove(object Item)
        {
            OnRemove((T)Item);
        }

        protected virtual void OnRemove(T Item)
        {
            Remove<E, T>(Item);
        }

        protected void Remove<ET,TT>(TT Item)
        {
            var entityItem = AutoMapper.Mapper.Map<TT, ET>((TT)Item);

            var entity = (ET)Context.GetObjectByKey(GetEntityKey(entityItem));
           
            Context.DeleteObject(entity);

            Context.ObjectStateManager.ChangeObjectState(entity, System.Data.EntityState.Deleted);

            Context.SaveChanges();
        }

        #region IRepositoryEntity

        private ObjectStateEntry GetEntityStateEntry(object ServiceModel)
        {            
            var entity = AutoMapper.Mapper.Map(ServiceModel, typeof(T), typeof(E));

            return Context.ObjectStateManager.GetObjectStateEntries(EntityState.Deleted | EntityState.Modified | EntityState.Added).SingleOrDefault(p => GetEntityKey(p.Entity) == GetEntityKey(entity));
        }

        object IRepositoryEntity.GetEntityKey(object ServiceModel)
        {
            var entry = GetEntityStateEntry(ServiceModel);
            
            return GetEntityKey(entry.Entity).EntityKeyValues[0].Value;
        }

        EntityState IRepositoryEntity.GetEntityState(object ServiceModel)
        {
            var entry = GetEntityStateEntry(ServiceModel);

            return entry.State;
        }

        #endregion

        protected virtual void OnTransactionCommited()
        {
        }

    }
}
