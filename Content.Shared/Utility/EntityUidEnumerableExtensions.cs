#nullable enable
using System.Collections.Generic;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

namespace Content.Shared.Utility
{
    public static class EntityUidEnumerableExtensions
    {
        public static List<IEntity> TryToEntities(this IEnumerable<EntityUid> ids, IEntityManager? entityManager = null)
        {
            entityManager ??= IoCManager.Resolve<IEntityManager>();

            var entities = new List<IEntity>();

            foreach (var id in ids)
            {
                if (entityManager.TryGetEntity(id, out var entity))
                {
                    entities.Add(entity);
                }
            }

            return entities;
        }

        public static List<IEntity> TryToEntities(this EntityUid[] ids, IEntityManager? entityManager = null)
        {
            entityManager ??= IoCManager.Resolve<IEntityManager>();

            var entities = new List<IEntity>(ids.Length);

            foreach (var id in ids)
            {
                if (entityManager.TryGetEntity(id, out var entity))
                {
                    entities.Add(entity);
                }
            }

            return entities;
        }

        public static List<IEntity> TryToEntities(this IReadOnlyList<EntityUid> ids, IEntityManager? entityManager = null)
        {
            entityManager ??= IoCManager.Resolve<IEntityManager>();

            var entities = new List<IEntity>(ids.Count);

            foreach (var id in ids)
            {
                if (entityManager.TryGetEntity(id, out var entity))
                {
                    entities.Add(entity);
                }
            }

            return entities;
        }

        public static List<T> TryToComponents<T>(
            this IEnumerable<EntityUid> ids,
            IEntityManager? entityManager = null)
            where T : class, IComponent
        {
            entityManager ??= IoCManager.Resolve<IEntityManager>();

            var components = new List<T>();

            foreach (var id in ids)
            {
                if (entityManager.TryGetEntity(id, out var entity) &&
                    entity.TryGetComponent(out T? component))
                {
                    components.Add(component);
                }
            }

            return components;
        }

        public static List<T> TryToComponents<T>(
            this EntityUid[] ids,
            IEntityManager? entityManager = null)
            where T : class, IComponent
        {
            entityManager ??= IoCManager.Resolve<IEntityManager>();

            var components = new List<T>(ids.Length);

            foreach (var id in ids)
            {
                if (entityManager.TryGetEntity(id, out var entity) &&
                    entity.TryGetComponent(out T? component))
                {
                    components.Add(component);
                }
            }

            return components;
        }

        public static List<T> TryToComponents<T>(
            this IReadOnlyList<EntityUid> ids,
            IEntityManager? entityManager = null)
            where T : class, IComponent
        {
            entityManager ??= IoCManager.Resolve<IEntityManager>();

            var components = new List<T>(ids.Count);

            foreach (var id in ids)
            {
                if (entityManager.TryGetEntity(id, out var entity) &&
                    entity.TryGetComponent(out T? component))
                {
                    components.Add(component);
                }
            }

            return components;
        }
    }
}
