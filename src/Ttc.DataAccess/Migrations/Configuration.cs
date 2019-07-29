using System.Collections.Generic;
using System.Data.Entity.Migrations.Design;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Utilities;
using Frenoy.Api;
using Ttc.DataEntities;
using Ttc.Model.Players;
using Ttc.Model.Teams;

namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Ttc.DataAccess.TtcDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        /// <see cref="NewSeasonSeed"/>
        protected override void Seed(TtcDbContext context)
        {
            // New season seed is now done from the frontend
        }
    }
}
