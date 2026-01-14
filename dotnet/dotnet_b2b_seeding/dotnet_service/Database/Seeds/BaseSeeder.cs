using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DotnetService.Database.Models;

namespace DotnetService.Database.Seeds
{
    /// <summary>
    /// Base seeder class providing common functionality for all seeders.
    /// Similar to the Python base_seeder.py implementation.
    /// </summary>
    public abstract class BaseSeeder
    {
        protected readonly DatabaseContext Context;
        protected readonly int BatchSize;
        protected readonly bool Verbose;
        protected int CreatedCount = 0;
        
        public BaseSeeder(DatabaseContext context, int batchSize = 1000, bool verbose = true)
        {
            Context = context;
            BatchSize = batchSize;
            Verbose = verbose;
        }
        
        public abstract Task<int> SeedAsync(int count);
        
        protected async Task BatchInsertAsync<T>(List<T> objects) where T : class
        {
            for (int i = 0; i < objects.Count; i += BatchSize)
            {
                var batch = objects.Skip(i).Take(BatchSize).ToList();
                
                try
                {
                    await Context.Set<T>().AddRangeAsync(batch);
                    await Context.SaveChangesAsync();
                    
                    CreatedCount += batch.Count;
                    
                    if (Verbose)
                    {
                        Console.WriteLine($"Inserted batch of {batch.Count} records. Total created: {CreatedCount}");
                    }
                }
                catch (Exception error)
                {
                    var errorStr = error.Message.ToLower();
                    if (errorStr.Contains("duplicate") || errorStr.Contains("unique") || errorStr.Contains("integrity"))
                    {
                        Console.WriteLine("[WARN] Duplicate entry detected in batch. Attempting individual inserts...");
                        
                        int successCount = 0;
                        foreach (var obj in batch)
                        {
                            try
                            {
                                Context.Entry(obj).State = EntityState.Added;
                                await Context.SaveChangesAsync();
                                successCount++;
                                CreatedCount++;
                            }
                            catch (Exception individualError)
                            {
                                Context.Entry(obj).State = EntityState.Detached;
                                var errorStrInd = individualError.Message.ToLower();
                                if (errorStrInd.Contains("duplicate") || errorStrInd.Contains("unique"))
                                {
                                    if (Verbose)
                                    {
                                        Console.WriteLine("[WARN] Skipping duplicate entry");
                                    }
                                }
                                else
                                {
                                    if (Verbose)
                                    {
                                        Console.WriteLine($"[WARN] Error inserting record: {individualError.Message}");
                                    }
                                }
                            }
                        }
                        
                        if (Verbose)
                        {
                            Console.WriteLine($"Inserted {successCount} of {batch.Count} records from batch. Total created: {CreatedCount}");
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
        
        protected DateTime GetRandomDate(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.UtcNow.AddDays(-365);
            endDate ??= DateTime.UtcNow;
            
            var random = new Random();
            var timeSpan = endDate.Value - startDate.Value;
            var randomDays = random.Next(0, timeSpan.Days);
            var randomTime = random.Next(0, 86400);
            
            return startDate.Value.AddDays(randomDays).AddSeconds(randomTime);
        }
        
        protected T? GetRandomChoice<T>(List<T> choices)
        {
            if (choices == null || choices.Count == 0)
                return default(T);
            
            var random = new Random();
            return choices[random.Next(choices.Count)];
        }
        
        protected bool GetRandomBoolean(double trueProbability = 0.5)
        {
            var random = new Random();
            return random.NextDouble() < trueProbability;
        }
        
        public int GetCreatedCount() => CreatedCount;
    }
}

