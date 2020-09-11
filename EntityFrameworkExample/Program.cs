using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Migrations;

namespace EntityFrameworkExample
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new MovieContext()) //no connection string, use VS default
            {
                var movie = new Movie
                {
                    Name = "Ground Hog Day",
                    Year = 1992
                };
                db.Movies.Add(movie);
                db.SaveChanges();

                var rating = new Rating
                {
                    RatingDate = Convert.ToDateTime("09/02/1991"),
                    RatingValue = 4,
                    Rater = "Rotten Tomatoes",
                    MovieId = movie
                };
                db.Ratings.Add(rating);

                //Add a second rating for the same movie to confirm a one to many relationship:
                rating = new Rating
                {
                    RatingDate = Convert.ToDateTime("09/21/1991"),
                    RatingValue = 3,
                    Rater = "Rolling Stone",
                    MovieId = movie
                };
                db.Ratings.Add(rating);
                db.SaveChanges();

                var query1 = from m in db.Movies orderby m.MovieId select m;
                //t-SQL: select m.* from Movies m order by m.MovieId 

                //query1 now is this SQL:
                //"SELECT \r\n    
                //[Extent1].[MovieId] AS [MovieId], \r\n    
                //[Extent1].[Name] AS [Name], \r\n    
                //[Extent1].[Year] AS [Year]\r\n    
                //FROM [dbo].[Movies] AS [Extent1]\r\n    
                //ORDER BY [Extent1].[MovieId] ASC"

                foreach (var item in query1)
                {
                    Console.WriteLine(item.MovieId + " " + item.Name + " " + item.Year);
                }

                var query2 = from r in db.Ratings orderby r.RatingDate select r;

                /* query2's actual SQL:
                "SELECT \r\n    
                [Extent1].[RatingId] AS [RatingId], \r\n    
                [Extent1].[Rater] AS [Rater], \r\n    
                [Extent1].[RatingValue] AS [RatingValue], \r\n    
                [Extent1].[RatingDate] AS [RatingDate], \r\n    
                [Extent1].[MovieId_MovieId] AS [MovieId_MovieId]\r\n    
                FROM [dbo].[Ratings] AS [Extent1]\r\n    
                ORDER BY [Extent1].[RatingDate] ASC"
                */

                foreach (var item in query2)
                {
                    Console.WriteLine(item.RatingId + " " + item.Rater + " " + 
                        item.RatingValue + " " + item.RatingDate + " " + item.MovieId.MovieId);
                        //Since each "item" is a Ratings object, then item.MovieId is actually a Movie object.
                        //To get the MovieId, I need the MovieId property of the MovieId object of "item"
                };

                int maxRating = db.Ratings.Max(p => p.RatingValue);
                Console.WriteLine("Max Rating: " + maxRating.ToString());
                DateTime maxDate = db.Ratings.Max(p => p.RatingDate);
                Console.WriteLine("Max Rating Date: " + maxDate.ToString());


                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }

        }
    }

    public class Movie
    {
        public int MovieId { get; set; } //this will be the primary key
        public string Name { get; set; }
        public int Year { get; set; }
    }

    public class Rating
    { 
        public int RatingId { get; set; } //this will be the primary key
        public string Rater { get; set; }
        public int RatingValue { get; set; }
        public DateTime RatingDate { get; set; }
        public Movie MovieId { get; set; } //this will be a foreign key
    }

    public class MovieContext : DbContext 
    {
        public MovieContext() { }
        public MovieContext(string connString)
        {
            Database.Connection.ConnectionString = connString;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //First Stored Procedure creation:
            //modelBuilder.Entity<Movie>().MapToStoredProcedures();

            //Second version, renaming your procedures
            modelBuilder.Entity<Movie>().MapToStoredProcedures
            (p => p.Insert(sp => sp.HasName ("sp_InsertMovie").Parameter(pm => pm.Name, "name").Result(rs => rs.MovieId, "Id"))
                .Update(sp => sp.HasName ("sp_UpdateMovie").Parameter(pm => pm.Name, "name"))
                .Delete(sp => sp.HasName ("sp_DeleteMovie").Parameter(pm => pm.MovieId, "Id"))
            );

        }


        public DbSet<Movie> Movies { get; set; }
        public DbSet<Rating> Ratings { get; set; }
    }


}













