// See https://aka.ms/new-console-template for more information
using System.Linq.Expressions;

Console.WriteLine("Hello, World!");

MyDbContext myDbContext = new();
QueryBuilder queryBuilder = new QueryBuilder();
IQueryable<Stok> query = myDbContext.Stok.AsQueryable();
IQueryable<Referans> refQuery = myDbContext.Referans.AsQueryable();
var queryStok = queryBuilder.BuildDynamicQuery(query, refQuery, "Açıklama1", myDbContext);
var stokList = queryStok.ToList();

Console.WriteLine("Stok Adet:" + stokList.Count);
Console.ReadLine();


class Stok
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Kod1 { get; set; }
    public Referans Kod1Ref { get; set; }
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


//x => x.Kod1.Aciklama1  
//x => _dbContext.Referans.Where(x => x.Aciklama1 == "").Select(x => x.Kod).FirstOrDefault()  => Kod
//x => x.Kod1 == Kod 

class MyDbContext
{
    public MyDbContext()
    {
        Referans referans = new Referans();
        referans.Kod = "Test1";
        referans.Aciklama1 = "Açıklama1";
        Referans.Add(referans);

        Referans referans2 = new Referans();
        referans2.Kod = "Test2";
        referans2.Aciklama1 = "Açıklama2";
        Referans.Add(referans2);

        Stok stok = new Stok();
        stok.Id = Guid.NewGuid();
        stok.Kod1 = "Test1";
        Stok.Add(stok);

        Stok stok2 = new Stok();
        stok2.Id = Guid.NewGuid();
        stok2.Kod1 = "Deneme";
        Stok.Add(stok2);

        Stok stok3 = new Stok();
        stok3.Id = Guid.NewGuid();
        stok3.Kod1 = "Test1";
        stok3.Name = "Name";
        Stok.Add(stok3);
    }
    public List<Referans> Referans { get; set; } = new();
    public List<Stok> Stok { get; set; } = new();
}

class QueryBuilder
{
    //Kod1.Aciklama1::==::Test::##
    public IQueryable<Stok> BuildDynamicQuery(IQueryable<Stok> query, IQueryable<Referans> refQuery, string refAciklama, MyDbContext dbContext)
    {
        // Create parameter expression for the Stok entity
        ParameterExpression stokParameter = Expression.Parameter(typeof(Stok), "x"); // x => x

        // Access property "Kod1" from the entity
        MemberExpression stokProperty = Expression.Property(stokParameter, "Kod1"); // x => x.Kod1



        // Create constant expression for the value of refAciklama
        ConstantExpression constantAciklama = Expression.Constant(refAciklama, typeof(string));


        // Create parameter expression for the Referans entity
        ParameterExpression refParameter = Expression.Parameter(typeof(Referans), "rf"); // rf => rf

        // Access property "Aciklama1" from the Referans entity
        MemberExpression refAciklamaProperty = Expression.Property(refParameter, "Aciklama1");  // rf => rf.Aciklama1

        // Access property "Kod" from the Referans entity
        MemberExpression refKodProperty = Expression.Property(refParameter, "Kod"); // rf => rf.Kod

        // Create a subquery to get the Kod from Referans based on the provided refAciklama
        var subQuery = refQuery.AsQueryable()
            .Where(Expression.Lambda<Func<Referans, bool>>(Expression.Equal(refAciklamaProperty, constantAciklama), refParameter));

        var kodFromRef = subQuery.Select(Expression.Lambda<Func<Referans, string>>(refKodProperty, refParameter)).FirstOrDefault();
        ConstantExpression constantKod = Expression.Constant(kodFromRef, typeof(string));

        var lambda = Expression.Lambda<Func<Stok, bool>>(Expression.Equal(stokProperty, constantKod), stokParameter);
        
        // Create the main query using the subquery
        var mainQuery = query.AsQueryable()
            .Where(Expression.Lambda<Func<Stok, bool>>(Expression.Equal(stokProperty, constantKod), stokParameter));

        return mainQuery;
    }
}

#region Join Denemeleri
//class DynamicJoinBuilder
//{
//    public static IQueryable<Stok> BuildLeftJoinQuery(IQueryable<Stok> stokQuery, string filter)
//    {
//        ParameterExpression stokParam = Expression.Parameter(typeof(Stok), "stok");
//        ParameterExpression referansParam = Expression.Parameter(typeof(Referans), "referans");

//        Expression<Func<Referans, bool>> referansFilter = x => x.Aciklama1 == filter;

//        var referansQuery = _tenantDbContext.Referans
//            .Where(referansFilter)
//            .AsQueryable();

//        Expression joinExpression = BuildLeftJoinExpression(stokParam, referansParam, referansQuery);

//        var resultQuery = stokQuery.Provider.CreateQuery<Stok>(joinExpression);

//        return resultQuery;
//    }

//    private static Expression BuildLeftJoinExpression(ParameterExpression stokParam, ParameterExpression referansParam, IQueryable<Referans> referansQuery)
//    {
//        // Build the dynamic left join expression
//        return Expression.Call(
//            typeof(Queryable), "GroupJoin",
//            new Type[] { typeof(Stok), typeof(Referans), typeof(int), typeof(IQueryable<Referans>) },
//            stokQuery.Expression, referansQuery.Expression,
//            Expression.Lambda<Func<Stok, int>>(Expression.Property(stokParam, "Kod1"), stokParam),
//            Expression.Lambda<Func<Referans, int>>(Expression.Property(referansParam, "Kod"), referansParam),
//            Expression.Quote(Expression.Lambda<Func<Stok, IEnumerable<Referans>>>(Expression.Constant(null), stokParam))
//        );

//    }
//}
#endregion




#region Enumarable?? Filter
//if (prop.Type.Name.Contains("List"))
//{
//    ParameterExpression parameter = Expression.Parameter(prop.Member.DeclaringType, "x");
//    MemberExpression enumabableMember = Expression.Property(parameter, prop.Member.Name);
//    Type enumarableMemberType = enumabableMember.Member.ReflectedType;

//    var anyMethod = typeof(Enumerable)
//        .GetMethods()
//        .First(m => m.Name == "Any" && m.GetParameters().Length == 1)
//        .MakeGenericMethod(enumarableMemberType);

//    var stokBelgeParameter = Expression.Parameter(enumarableMemberType, "y");
//    var condition = Expression.Equal(Expression.Property(stokBelgeParameter, item), Expression.Constant(filterNode.Value));

//    var anyCall = Expression.Call(anyMethod, enumabableMember, Expression.Lambda(condition, stokBelgeParameter));

//    var lambda2 = Expression.Lambda(anyCall, parameter);

//    return lambda2;

//}
//else
//{
//    prop = Expression.PropertyOrField(prop, item);
//}
#endregion



