Console.WriteLine("Fun with Entity Framework Core");
static void SampleSaveChanges()
{
    //The factory is not meant to be used like this, but it’s demo code :-)
    var context = new ApplicationDbContextFactory().CreateDbContext(null);
    //make some changes
    context.SaveChanges();
}
