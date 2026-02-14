namespace BitacoraAlfipac.Data.Seed
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            UserSeed.SeedAdminUser(context);
            CatalogSeed.SeedCatalogs(context);
        }
    }
}
