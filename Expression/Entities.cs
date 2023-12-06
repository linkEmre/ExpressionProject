using System.Linq.Expressions;

namespace ExpressionProject;

class Stok
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid StokBelgeId { get; set; }
    public StokBelge StokBelge { get; set; }

}

class StokBelge
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string No { get; set; }
}

class Referans
{
    public string Kod { get; set; }
    public string Aciklama1 { get; set; }
    public string Aciklama2 { get; set; }
    public string GrupKod { get; set; }
    public float? SayKod { get; set; }
    public int TipId { get; set; }
}

class MyDbContext 
{
    public List<Referans> Referans { get; set; } = new List<Referans>();
    public List<Stok> Stok { get; set; } = new();
}

class QueryBuilder
{
    public IQueryable<Stok> BuildDynamicQuery(string refAciklama, MyDbContext dbContext)
    {
        // Create parameter expression for the entity
        ParameterExpression parameter = Expression.Parameter(typeof(Stok), "x");

        // Access property "Kod1" from the entity
        MemberExpression property = Expression.Property(parameter, "Kod1");

        // Create constant expression for the value of refAciklama
        ConstantExpression constant = Expression.Constant(refAciklama, typeof(string));

        // Access property "Aciklama1" from the Rr entity
        MemberExpression rrProperty = Expression.Property(Expression.Parameter(typeof(Referans), "rf"), "Aciklama1");

        // Create a subquery to get the Kod from Rr based on the provided refAciklama
        var subQuery = dbContext.Referans.AsQueryable()
            .Where(rf => rrProperty == constant.Value)
            .Select(Expression.Lambda<Func<Referans, int>>(Expression.Property(Expression.Parameter(typeof(Referans), "x"), "Kod"), parameter));

        // Create the main query using the subquery
        var mainQuery = dbContext.Stok.AsQueryable()
            .Where(Expression.Lambda<Func<Stok, bool>>(Expression.Equal(property, subQuery.Expression), parameter));

        return mainQuery;
    }
}
