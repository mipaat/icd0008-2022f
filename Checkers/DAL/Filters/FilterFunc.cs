using Domain;

namespace DAL.Filters;

public delegate IQueryable<T> FilterFunc<T>(IQueryable<T> arg) where T : IDatabaseEntity;