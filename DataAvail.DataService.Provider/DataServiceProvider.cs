using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Data.Services.Toolkit.Providers;
using Microsoft.Data.Services.Toolkit.QueryModel;
using System.Reflection;
using System.Data.Objects;
using System.Web;

namespace DataAvail.DataService.Provider
{

    public abstract class DataServiceProvider : ODataContext
    {
        public DataServiceProvider(string RepositoryTypeTemplate)
            : this(null, RepositoryTypeTemplate)
        {
        }
        
        public DataServiceProvider(object Context, string RepositoryTypeTemplate)
            : this(Context, null, RepositoryTypeTemplate)
        {
        }

        public DataServiceProvider(object Context, Assembly Assembly, string RepositoryTypeTemplate)
        {
            _assembly = Assembly ?? Assembly.GetAssembly(this.GetType());
            _repositoryTypeTemplate = RepositoryTypeTemplate;
            SetContext(Context);
        }

        private object _context;
        private readonly Assembly _assembly;
        private readonly string _repositoryTypeTemplate;

        protected void SetContext(object Context)
        {
            _context = Context;
        }

        public object Context { get { return _context; } }

        public IContext IContext { get { return _context as IContext; } }

        public object DataSource { get { return IContext != null ? IContext.DataSource : Context; } }

        public static dynamic GetCustomFilter()
        {
            var customFilter = GetQueryStringParam("customFilter");

            if (customFilter != null)
            {
                //TO DO : temp fix
                customFilter = customFilter.Replace("$", "_");

                JsonFx.Serialization.DataReaderSettings sgs = new JsonFx.Serialization.DataReaderSettings();

                var jsonReader = new JsonFx.Json.JsonReader(sgs);

                return jsonReader.Read(customFilter);
            }
            else
            {
                return null;
            }
        }

        public static string GetQueryStringParam(string ParamName)
        {
            if (HttpContext.Current != null)
            {
                var queryString = HttpContext.Current.Request.QueryString;

                if (queryString[ParamName] != null)
                {
                    return queryString[ParamName];
                }
            }

            return null;
        }

        public override object RepositoryFor(string fullTypeName)
        {

            string typeName = fullTypeName.Replace("[]", string.Empty).Substring(fullTypeName.LastIndexOf('.') + 1);
            Type repoType = _assembly.GetType(string.Format(_repositoryTypeTemplate , typeName));
            if (repoType == null) throw new NotSupportedException("The specified type is not an Entity inside the OData API");
            object repository = Activator.CreateInstance(repoType);
            if (repository is IRepository)
                ((IRepository)repository).SetContext(DataSource);
            return repository;
        }

        /*
        public override void ClearChanges()
        {
            System.Diagnostics.Debug.WriteLine("Error while update");
        }
         */

        public override void SaveChanges()
        {
            //return;

            string typeName = null;

            NotifyItemChangedData? notifyData = null;


            if (IContext != null)
            {
                IContext.BeginTransaction();

                try
                {
                    var rootSaveContext = this.GetRootSaveContext();

                    base.SaveChanges();
                    
                    typeName = rootSaveContext.Value.GetType().Name;

                    if (rootSaveContext.Key is IRepositoryValidate)
                    {
                        ((IRepositoryValidate)rootSaveContext.Key).ValidateUpdate(rootSaveContext.Value);
                    }

                    notifyData = GetNotifyData(rootSaveContext.Value);
                }
                catch (System.Exception e)
                {
                    IContext.RollbackTransaction();

                    throw e;
                }

                IContext.CommitTransaction();
            }
            else
            {
                base.SaveChanges();
            }


            if (notifyData != null && typeName != null && notifyData.Value.state != ItemChangedState.Unchanged) NotifyItemChanged(typeName, (NotifyItemChangedData)notifyData);
        }

        public override void ClearChanges()
        {            
        }

        #region Notify

        private void NotifyItemChanged(string TypeName, NotifyItemChangedData NotifyData)
        {
            if (Context is IContextNotifier)
            {
                ((IContextNotifier)Context).NotfyItemChanged(TypeName, NotifyData);
            }
        }

        protected virtual NotifyItemChangedData? GetNotifyData(object Entity)
        {
            /*
            var repository = RepositoryFor(Entity.GetType().FullName);

            if (repository is IRepositoryEntity)
            {
                var er = (IRepositoryEntity)repository;

                return new NotifyItemChangedData { key = er.GetEntityKey(Entity), state = GetItemChangedState(er.GetEntityState(Entity)) };
            }
             */

            return null;
        }

        private static ItemChangedState GetItemChangedState(System.Data.EntityState EntityState)
        {
            switch (EntityState)
            { 
                case System.Data.EntityState.Modified:
                    return ItemChangedState.Modified;
                case System.Data.EntityState.Deleted:
                    return ItemChangedState.Deleted;
                case System.Data.EntityState.Added:
                    return ItemChangedState.Added;
                default:
                    return ItemChangedState.Unchanged;
            }
        }

        #endregion

    }
}
