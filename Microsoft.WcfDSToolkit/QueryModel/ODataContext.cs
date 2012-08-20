namespace Microsoft.Data.Services.Toolkit.QueryModel
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Providers;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.Data.Services.Toolkit.Providers;
    using System.Collections;

    /// <summary>
    /// A custom implementation for the <see cref="IDataServiceUpdateProvider"/> contract.
    /// </summary>
    public abstract class ODataContext : IDataServiceUpdateProvider, IPageableDataContext
    {
        private object _currentEntity;
        private string _currentOperation;
        private object _currentRelatedEntity;


        private class BatchContext
        {
            internal BatchContext(object Entity, string Operation)
            {
                this.Entity = Entity;

                this.Operation = Operation;
            }

            internal object Entity { get; set; }

            internal string Operation { get; set; }

            internal object RelatedEntity { get; set; }

            internal string RelatedProperty { get; set; }
        }

        private IDictionary<string, string> _queryableTypeNames;

        private List<BatchContext> _batchContexts;

        public ODataContext()
        {
            _queryableTypeNames = new Dictionary<string, string>();

            _batchContexts = new List<BatchContext>();
        }

        /// <summary>
        /// Creates an <see cref="IQueryable"/> for a given type.
        /// </summary>
        /// <typeparam name="T">The <see cref="IQueryable"/> type.</typeparam>
        /// <returns>An <see cref="IQueryable"/> for a given type.</returns>
        public IQueryable<T> CreateQuery<T>()
        {
            if (!_queryableTypeNames.ContainsKey(typeof(T).FullName))
            {
                _queryableTypeNames.Add(typeof(T).FullName, typeof(T).AssemblyQualifiedName);
            }

            return new ODataQuery<T>(new ODataQueryProvider<T>(this.RepositoryFor));
        }

        /// <summary>
        /// Evaluates a custom <see cref="ExpressionVisitor"/> to get a resource.
        /// </summary>
        /// <param name="query">The <see cref="IQueryable"/> which contains the 
        /// expression to get the resource.</param>
        /// <param name="fullTypeName">The resource full type name.</param>
        /// <returns>The expected resource.</returns>
        public object GetResource(IQueryable query, string fullTypeName)
        {
            var visitor = new ODataExpressionVisitor(query.Expression);
            var operation = visitor.Eval() as ODataSelectOneQueryOperation;

            if (operation != null)
            {
                this._currentEntity = query.Provider.Execute(query.Expression);

                if (operation.Key.StartsWith("temp-"))
                    this._currentOperation = "CreateResource";
            }

            return this._currentEntity;
        }


        /// <summary>
        /// Executes the repository method CreateDefaultEntity and sets the current operation value to 'CreateResource'.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="fullTypeName">The resource's full type name.</param>
        /// <returns>The new resource.</returns>
        public object CreateResource(string containerName, string fullTypeName)
        {
            var repository = this.RepositoryFor(fullTypeName);

            if (repository == null)
                return null;

            _currentEntity = ExecuteMethodIfExists(repository, "CreateDefaultEntity");
            _currentOperation = "CreateResource";

            if (_currentEntity == null)
            {
                var typeNameWithAssembly = _queryableTypeNames[fullTypeName];

                _currentEntity = Activator.CreateInstance(Type.GetType(typeNameWithAssembly));
            }

            _batchContexts.Add(new BatchContext(_currentEntity, _currentOperation));

            return _currentEntity;
        }

        /// <summary>
        /// Assigns to the current entity the resource to be removed and sets the current operation value to 'DeleteResource'.
        /// </summary>
        /// <param name="targetResource">The resource to be removed.</param>
        public void DeleteResource(object targetResource)
        {
            _currentOperation = "DeleteResource";
            _currentEntity = targetResource.WrapIntoEnumerable().First();

            _batchContexts.Add(new BatchContext(_currentEntity, _currentOperation));
        }

        /// <summary>
        /// Sets the current operation value to 'ModifyResource' and wraps 
        /// the resource into an enumerable returning the first one.
        /// </summary>
        /// <param name="resource">The target resource.</param>
        /// <returns>The first resource in the collection.</returns>
        public object ResetResource(object resource)
        {
            _currentOperation = "ModifyResource";
            _batchContexts.Add(new BatchContext(resource, _currentOperation));
            return resource.WrapIntoEnumerable().First();
        }

        /// <summary>
        /// Wraps the resource into an enumerable and returns the first one.
        /// </summary>
        /// <param name="resource">The current resource.</param>
        /// <returns>The first resource in the collection.</returns>
        public object ResolveResource(object resource)
        {
            return resource.WrapIntoEnumerable().First();
        }

        /// <summary>
        /// Sets every resource value by reflection.
        /// </summary>
        /// <param name="targetResource">The target resource.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="propertyValue">The property value.</param>
        public void SetValue(object targetResource, string propertyName, object propertyValue)
        {
            _currentOperation = "ModifyResource";
            var resource = targetResource.WrapIntoEnumerable().First();
            var pty = resource.GetType().GetProperty(propertyName);

            if (!pty.CanWrite)
                return;

            pty.SetValue(resource, propertyValue, null);
        }

        /// <summary>
        /// Gets every resource value by reflection.
        /// </summary>
        /// <param name="targetResource">The target resource.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The resource property.</returns>
        public object GetValue(object targetResource, string propertyName)
        {
            var resource = targetResource.WrapIntoEnumerable().First();
            var property = resource.GetType().GetProperty(propertyName);

            return property.CanRead ? property.GetValue(resource, null) : null;
        }

        private bool IsSameEntities(object Enty1, object Enty2)
        {
            var k1 = Enty1.DataServiceKeys();
            var k2 = Enty2.DataServiceKeys();

            return Enty1.GetType() == Enty2.GetType() && k1.Values.First() == k2.Values.First();
        }
        
        /// <summary>
        /// Calls the corresponding operation based on the current operation field.
        /// </summary>
        public virtual void SaveChanges()
        {
            List<object> pervItems = new List<object>();

            foreach (var batchContext in _batchContexts)
            {
                _currentEntity = batchContext.Entity.WrapIntoEnumerable().First();
                _currentRelatedEntity = batchContext.RelatedEntity;
                _currentOperation = batchContext.Operation;

                var pervEnty = pervItems.FirstOrDefault(p => IsSameEntities(p, _currentEntity));

                /*
                 * Since, for update root item (from PUT changeset) is setup directly from request 
                 * and for $links these items are retrieved from data source. Make them all reference 
                 * to a single root item.
                 */
                if (pervEnty != null)
                {
                    _currentEntity = pervEnty;
                }
                else
                {
                    pervItems.Add(_currentEntity);
                }

                if (_currentOperation == null)
                    return;

                var repository = RepositoryFor(_currentEntity.GetType().FullName);

                if (repository == null)
                    return;

                if (_currentOperation == "CreateResource" || _currentOperation == "ModifyResource")
                    ExecuteMethodIfExists(repository, "Save", _currentEntity);
                
                else if (_currentOperation == "DeleteResource")
                    ExecuteMethodIfExists(repository, "Remove", _currentEntity);

                else if (_currentOperation == "AddReferenceToCollection")
                    ExecuteMethodIfExists(repository, "CreateRelation", _currentEntity, _currentRelatedEntity, batchContext.RelatedProperty);

                else if (_currentOperation == "RemoveReferenceFromCollection")
                    ExecuteMethodIfExists(repository, "DeleteRelation", _currentEntity, _currentRelatedEntity, batchContext.RelatedProperty);

                else if (_currentOperation == "SetReference")
                    ExecuteMethodIfExists(repository, "SetRelation", _currentEntity, _currentRelatedEntity, batchContext.RelatedProperty);
            }
        }

        private object ExecuteMethodIfExists(object repository, string methodName, params object[] parameterValues)
        {
            var methodSignature = parameterValues.Select(pv => pv == null ? typeof(object) : pv.GetType()).ToArray();
            var method = repository.GetType().GetMethod(methodName, methodSignature);

            if (method != null)
            {
                if (method.ReturnType != typeof(void))
                    return method.Invoke(repository, parameterValues);

                method.Invoke(repository, parameterValues);
            }

            return null;
        }

        /// <summary>
        /// Throws <see cref="NotImplementedException"/>.
        /// </summary>
        public virtual void ClearChanges()
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Throws <see cref="NotImplementedException"/>.
        /// </summary>
        /// <param name="targetResource">The target resource.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="propertyValue">The property value.</param>
        public void SetReference(object targetResource, string propertyName, object propertyValue)
        {
            //_currentOperation = "ModifyResource";
            _currentOperation = "SetReference";
            _currentEntity = targetResource;

            var resource = targetResource.WrapIntoEnumerable().First();
            var toBeSet = propertyValue != null ?  propertyValue.WrapIntoEnumerable().First() : null;
            var property = resource.GetType().GetProperty(propertyName);

            if (!property.CanWrite)
                return;

            _batchContexts.Add(new BatchContext(resource, _currentOperation) { RelatedEntity = toBeSet, RelatedProperty = propertyName });

        }

        /// <summary>
        /// Sets the current operation as 'AddReferenceToCollection' and the involved entities references.
        /// </summary>
        /// <param name="targetResource">The target resource.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="resourceToBeAdded">The resource to be added.</param>
        public void AddReferenceToCollection(object targetResource, string propertyName, object resourceToBeAdded)
        {
            this._currentOperation = "AddReferenceToCollection";
            this._currentEntity = targetResource;
            this._currentRelatedEntity = resourceToBeAdded;

            var resource = targetResource.WrapIntoEnumerable().First();
            var toBeAdded = resourceToBeAdded.WrapIntoEnumerable().First();

            _batchContexts.Add(new BatchContext(resource, _currentOperation) { RelatedEntity = toBeAdded, RelatedProperty = propertyName });
        }


        /// <summary>
        /// Throws <see cref="NotImplementedException"/>.
        /// </summary>
        /// <param name="targetResource">The target resource.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="resourceToBeRemoved">The resource to be removed from the collection.</param>
        public void RemoveReferenceFromCollection(object targetResource, string propertyName, object resourceToBeRemoved)
        {
            this._currentOperation = "RemoveReferenceFromCollection";
            this._currentEntity = targetResource;
            this._currentRelatedEntity = resourceToBeRemoved;

            var resource = targetResource.WrapIntoEnumerable().First();
            var toBeRemoved = resourceToBeRemoved.WrapIntoEnumerable().First();

            _batchContexts.Add(new BatchContext(resource, _currentOperation) { RelatedEntity = toBeRemoved, RelatedProperty = propertyName });
        }

        /// <summary>
        /// Sets concurrency values to avoid concurrency issues.
        /// </summary>
        /// <param name="resourceCookie">The resource cookie object.</param>
        /// <param name="checkForEquality">A value indicating whether it will check for equality or not.</param>
        /// <param name="concurrencyValues">The concurrency values to be analyzed.</param>
        public void SetConcurrencyValues(object resourceCookie, bool? checkForEquality, IEnumerable<KeyValuePair<string, object>> concurrencyValues)
        {
        }

        /// <summary>
        /// Obtains information about which repository will be used based on the entity full type name.
        /// </summary>
        /// <param name="fullTypeName">The entity full type name.</param>
        /// <returns>A repository for a given entity full type name.</returns>
        public abstract object RepositoryFor(string fullTypeName);

        public void SetPagingProvider(GenericPagingProvider pagingProvider)
        {
            PagingProvider = pagingProvider;
        }

        public GenericPagingProvider PagingProvider { get; private set; }

        public KeyValuePair<object, object> GetRootSaveContext()
        {
            var context = _batchContexts.FirstOrDefault();

            var enty = context.Entity.WrapIntoEnumerable().First();

            return new KeyValuePair<object, object>(RepositoryFor(enty.GetType().FullName), enty);

        }


    }
}