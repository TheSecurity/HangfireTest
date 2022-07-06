using Hangfire;
using HangfireTest.Api.Services;

namespace HangfireTest.Api
{
    public class JobFactory
    {
        private readonly IRecurringJobManager _recurringJobs;
        private readonly UserService _userService;

        public JobFactory(IRecurringJobManager recurringJobs, UserService userService)
        {
            _recurringJobs = recurringJobs;
            _userService = userService;
        }

        public void CreateGetUserCountJob()
        {
            _recurringJobs.AddOrUpdate("user-count", () => GetUserCountJobAsync(), "0 * * ? * *");
        }

        public async Task GetUserCountJobAsync()
        {
            var count = await _userService.GetUserCountAsync();
            Console.WriteLine(count);
        }

        [AutomaticRetry(Attempts = 3, DelaysInSeconds = new int[] { 10, 30 }, LogEvents = true)]
        public void CreateFailingUserJob()
        {
            _recurringJobs.AddOrUpdate("user-fail", () => GetFailingJob(), "0 0/2 * ? * *");
        }

        public static void GetFailingJob()
        {
            throw new Exception("Failing job");
        }

        public void CreateLongUserJob()
        {
            _recurringJobs.AddOrUpdate("user-long-job", () => GetLongUserJobAsync(), "0 0/2 * ? * *");
        }

        public static async Task GetLongUserJobAsync()
        {
            await Task.Delay(30000);
        }

        public void RegisterAllJobs()
        {
            CreateFailingUserJob();
            CreateGetUserCountJob();
            CreateLongUserJob();
        }
    }
}
