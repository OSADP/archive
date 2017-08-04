namespace IDTO.Data.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Collections.Generic;
    using IDTO.Entity.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<IDTO.Data.IDTOContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(IDTO.Data.IDTOContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //

            //KG: the seed seems to run cuz it generated errors till i fixed it, but after
            //that no data ever appeared in the database. may need an initializerhttp://www.codeguru.com/csharp/article.php/c19999/Understanding-Database-Initializers-in-Entity-Framework-Code-First.htm
            //var modes = new List<Mode>
            //{
            //  new Mode { Id=1,Name = "Walk" },
            //  new Mode { Id=2,Name = "Bus" }
            //};
            //modes.ForEach(s => context.Modes.AddOrUpdate(p => p.Name, s));
            //context.SaveChanges();
//will need to add Id's like above to get below to work for AcceptChanges error
       //     var providertypes = new List<ProviderType>
       //     {
       //             new ProviderType
       // {
     
       //     Name =  "FixedRoute/TConnect" 
       // },
       // new ProviderType
       // {
         
       //     Name = "IncomingFixedRoute"
       // },
       // new ProviderType
       // {
        
       //     Name =  "Rideshare"
       // },
       // new ProviderType
       // {
      
       //     Name = "Demand/Response"
       // }
       //     };
       //     providertypes.ForEach(s => context.ProviderTypes.AddOrUpdate(p => p.Name, s));
       //     context.SaveChanges();

       //     var providers = new List<Provider>
       //{
       // new Provider
       // {
         
       //     Name = "COTA",
       //     ProviderTypeId = providertypes.Single( i => i.Name == "FixedRoute/TConnect" ).Id 
       // },
       // new Provider
       // {
        
       //     Name = "CapTrans",
       //     ProviderTypeId = providertypes.Single( i => i.Name ==  "Demand/Response" ).Id 
       // }
       //};


       //     providers.ForEach(s => context.Providers.AddOrUpdate(p => p.Name, s));
       //     context.SaveChanges();
        }
    }
}
