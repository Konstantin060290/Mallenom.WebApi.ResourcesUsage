using Mallenom.WebApi.ResourcesUsage.Services;
using Microsoft.AspNetCore.Mvc;

namespace Mallenom.WebApi.ResourcesUsage.Controllers
{
    [ApiController]
    [Route("api/metrics")]
    public class MetricsController : ControllerBase
    {
        private readonly MetricsService _metricsService;

        public MetricsController(MetricsService metricsService)
        {
            _metricsService = metricsService;
        }

        /// <summary>
        /// Get CPU loading metrics
        /// </summary>
        /// <remarks>
        /// Request example:       
        /// GET api/metrics/cpu
        /// </remarks>
        /// <returns>CPU loading metrics</returns>
        /// <response code="200">If is OK</response>
        [HttpGet("cpu")]
        public List<string?> GetCpuMetrics()
        {
            return _metricsService.GetCpuMetrics();
        }

        /// <summary>
        /// Get Memory usage metrics
        /// </summary>
        /// <remarks>
        /// Request example:       
        /// GET api/metrics/memory
        /// </remarks>
        /// <returns>Memory usage metrics</returns>
        /// <response code="200">If is OK</response>
        [HttpGet("memory")]
        public MemoryMetrics GetMemoryMetrics()
        {
            return _metricsService.GetMemoryMetrics();
        }
        /// <summary>
        /// Get Hard disks metrics
        /// </summary>
        /// <remarks>
        /// Request example:       
        /// GET api/metrics/disks
        /// </remarks>
        /// <returns>Hard disks metrics</returns>
        /// <response code="200">If is OK</response>
        [HttpGet("disks")]
        public DisksMetrics GetDisksMetrics()
        {
            return _metricsService.GetDisksMetrics();
        }

    }
}