using System;

namespace Recurly
{
    public abstract class RecurlyEntity
    {
        protected RecurlyEntity(string entityUri)
        {
            EntityUri = new Uri(entityUri, UriKind.Relative);
        }

        protected RecurlyEntity(Uri entityUri)
        {
            if(entityUri.IsAbsoluteUri)
                throw new ArgumentException("The entity uri should be relative to the API Uri");

            EntityUri = entityUri;
        }

        public Uri EntityUri { get; }

        protected bool Equals(RecurlyEntity other)
        {
            return Equals(EntityUri, other.EntityUri);
        }

        public override bool Equals(object obj)
        {
            if(obj is null)
                return false;
            if(ReferenceEquals(this, obj))
                return true;
            if(obj.GetType() != this.GetType())
                return false;
            return Equals((RecurlyEntity)obj);
        }

        public override int GetHashCode()
        {
            return (EntityUri != null ? EntityUri.GetHashCode() : 0);
        }

        public static bool operator ==(RecurlyEntity left, RecurlyEntity right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(RecurlyEntity left, RecurlyEntity right)
        {
            return !Equals(left, right);
        }

        protected static TEntity CreateNotFoundEntity<TEntity>() where TEntity : RecurlyEntity
        {
            var entity = Activator.CreateInstance(typeof(TEntity), "recurly://entity-not-found");
            return (TEntity)entity;
        }
    }
}