using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceCenterAPI.Application.Services;

namespace ServiceCenterAPI.Controllers;

/// <summary>
/// Jobs Controller - Manages jobs and job parts
/// Complete CRUD operations for Jobs and JobParts
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;

    public JobsController(IJobService jobService)
    {
        _jobService = jobService;
    }

    // ============ JOB GET ENDPOINTS ============

    /// <summary>
    /// Get Job with all its JobParts
    /// </summary>
    [HttpGet("{jobId}")]
    [ProducesResponseType(typeof(JobDetailedDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJob(int jobId)
    {
        if (jobId <= 0)
            return BadRequest(new { success = false, message = "Invalid job ID" });

        var job = await _jobService.GetJobDetailedAsync(jobId);
        if (job == null)
            return NotFound(new { success = false, message = $"Job with ID {jobId} not found" });

        return Ok(new { success = true, data = job, message = "Job retrieved successfully" });
    }

    /// <summary>
    /// Get all Jobs for an Item
    /// </summary>
    [HttpGet("item/{itemId}")]
    [ProducesResponseType(typeof(IEnumerable<JobDetailedDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetJobsByItem(int itemId)
    {
        if (itemId <= 0)
            return BadRequest(new { success = false, message = "Invalid item ID" });

        var jobs = await _jobService.GetJobsByItemAsync(itemId);
        return Ok(new { success = true, data = jobs, message = "Jobs retrieved successfully" });
    }

    // ============ JOB CREATE ENDPOINT ============

    /// <summary>
    /// Create Job for an Item
    /// </summary>
    [HttpPost("item/{itemId}")]
    [ProducesResponseType(typeof(JobDetailedDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateJob(int itemId, [FromBody] CreateJobDto dto)
    {
        if (itemId <= 0)
            return BadRequest(new { success = false, message = "Invalid item ID" });

        if (!ModelState.IsValid)
            return BadRequest(new { success = false, errors = ModelState });

        try
        {
            var job = await _jobService.AddJobToItemAsync(itemId, dto);
            return Created(new Uri($"api/jobs/{job.JobId}", UriKind.Relative),
                new { success = true, data = job, message = "Job created successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }

    // ============ JOB UPDATE ENDPOINT ============

    /// <summary>
    /// Update Job
    /// </summary>
    [HttpPut("{jobId}")]
    [ProducesResponseType(typeof(JobDetailedDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateJob(int jobId, [FromBody] UpdateJobDto dto)
    {
        if (jobId <= 0)
            return BadRequest(new { success = false, message = "Invalid job ID" });

        if (!ModelState.IsValid)
            return BadRequest(new { success = false, errors = ModelState });

        var job = await _jobService.UpdateJobAsync(jobId, dto);
        if (job == null)
            return NotFound(new { success = false, message = $"Job with ID {jobId} not found" });

        return Ok(new { success = true, data = job, message = "Job updated successfully" });
    }

    // ============ JOB DELETE ENDPOINT ============

    /// <summary>
    /// Delete Job (soft delete)
    /// Also deletes all job parts for this job
    /// </summary>
    [HttpDelete("{jobId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteJob(int jobId)
    {
        if (jobId <= 0)
            return BadRequest(new { success = false, message = "Invalid job ID" });

        var success = await _jobService.DeleteJobAsync(jobId);
        if (!success)
            return NotFound(new { success = false, message = $"Job with ID {jobId} not found" });

        return Ok(new { success = true, message = "Job deleted successfully" });
    }

    // ============ JOB PART GET ENDPOINTS ============

    /// <summary>
    /// Get all JobParts for a Job
    /// </summary>
    [HttpGet("{jobId}/parts")]
    [ProducesResponseType(typeof(IEnumerable<JobPartDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetJobParts(int jobId)
    {
        if (jobId <= 0)
            return BadRequest(new { success = false, message = "Invalid job ID" });

        var parts = await _jobService.GetJobPartsAsync(jobId);
        return Ok(new { success = true, data = parts, message = "Job parts retrieved successfully" });
    }

    // ============ JOB PART CREATE ENDPOINTS ============

    /// <summary>
    /// Add single JobPart to Job
    /// Auto-recalculates job actual cost
    /// </summary>
    [HttpPost("{jobId}/parts")]
    [ProducesResponseType(typeof(JobPartDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddJobPart(int jobId, [FromBody] CreateJobPartDto dto)
    {
        if (jobId <= 0)
            return BadRequest(new { success = false, message = "Invalid job ID" });

        if (!ModelState.IsValid)
            return BadRequest(new { success = false, errors = ModelState });

        try
        {
            var part = await _jobService.AddJobPartAsync(jobId, dto);
            return Created(new Uri($"api/jobparts/{part.JobPartId}", UriKind.Relative),
                new { success = true, data = part, message = "Job part created successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Bulk add JobParts to Job
    /// Auto-recalculates job actual cost
    /// </summary>
    [HttpPost("{jobId}/parts/bulk")]
    [ProducesResponseType(typeof(IEnumerable<JobPartDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BulkAddJobParts(int jobId, [FromBody] BulkAddJobPartsDto dto)
    {
        if (jobId <= 0)
            return BadRequest(new { success = false, message = "Invalid job ID" });

        if (dto == null || dto.JobParts == null || dto.JobParts.Count == 0)
            return BadRequest(new { success = false, message = "JobParts list cannot be empty" });

        if (!ModelState.IsValid)
            return BadRequest(new { success = false, errors = ModelState });

        try
        {
            var parts = await _jobService.BulkAddJobPartsAsync(jobId, dto.JobParts);
            return Created(new Uri($"api/jobs/{jobId}/parts", UriKind.Relative),
                new { success = true, data = parts, message = $"{parts.Count()} job parts added successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // ============ JOB PART UPDATE ENDPOINT ============

    /// <summary>
    /// Update JobPart
    /// Auto-recalculates job actual cost
    /// </summary>
    [HttpPut("parts/{jobPartId}")]
    [ProducesResponseType(typeof(JobPartDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateJobPart(int jobPartId, [FromBody] UpdateJobPartDto dto)
    {
        if (jobPartId <= 0)
            return BadRequest(new { success = false, message = "Invalid job part ID" });

        if (!ModelState.IsValid)
            return BadRequest(new { success = false, errors = ModelState });

        var part = await _jobService.UpdateJobPartAsync(jobPartId, dto);
        if (part == null)
            return NotFound(new { success = false, message = $"Job part with ID {jobPartId} not found" });

        return Ok(new { success = true, data = part, message = "Job part updated successfully" });
    }

    // ============ JOB PART DELETE ENDPOINT ============

    /// <summary>
    /// Delete JobPart
    /// Auto-recalculates job actual cost
    /// </summary>
    [HttpDelete("parts/{jobPartId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteJobPart(int jobPartId)
    {
        if (jobPartId <= 0)
            return BadRequest(new { success = false, message = "Invalid job part ID" });

        var success = await _jobService.DeleteJobPartAsync(jobPartId);
        if (!success)
            return NotFound(new { success = false, message = $"Job part with ID {jobPartId} not found" });

        return Ok(new { success = true, message = "Job part deleted successfully" });
    }
}