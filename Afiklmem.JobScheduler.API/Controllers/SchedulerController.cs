using Afiklmem.JobScheduler.BL;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class SchedulerController : ControllerBase
{
    private readonly JobScheduler _jobScheduler;
    private readonly IJobRepository _jobRepository;

    public SchedulerController(JobScheduler jobScheduler, IJobRepository jobRepository)
    {
        _jobScheduler = jobScheduler;
        _jobRepository = jobRepository;
    }

    // New: Add Job
    [HttpPost("add-job")]
    public async Task<IActionResult> AddJob([FromBody] Job job)
    {
        await _jobRepository.AddJobAsync(job);
        return Ok("Job added successfully");
    }

    // New: Delete Job
    [HttpDelete("delete-job/{id}")]
    public async Task<IActionResult> DeleteJob(int id)
    {
        var job = await _jobRepository.GetJobByIdAsync(id);
        if (job == null)
        {
            return NotFound("Job not found");
        }
        await _jobRepository.DeleteJobAsync(id);
        return Ok("Job deleted successfully");
    }

    // New: Get all jobs
    [HttpGet("get-all-jobs")]
    public async Task<IActionResult> GetAllJobs()
    {
        var jobs = await _jobRepository.GetAllJobsAsync();
        return Ok(jobs);
    }

    // New: Get one job
    [HttpGet("get-job/{id}")]
    public async Task<IActionResult> GetJobById(int id)
    {
        var job = await _jobRepository.GetJobByIdAsync(id);
        if (job == null)
        {
            return NotFound("Job not found");
        }
        return Ok(job);
    }

    // Existing: Get currently running jobs
    [HttpGet("running-jobs")]
    public IActionResult GetRunningJobs()
    {
        var runningJobs = _jobScheduler.GetCurrentlyRunningJobs();
        return Ok(runningJobs);
    }
}