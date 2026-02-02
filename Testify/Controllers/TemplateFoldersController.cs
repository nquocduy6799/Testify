using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Testify.Interfaces;
using Testify.Shared.DTOs.TemplateFolders;

namespace Testify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class TemplateFoldersController : ControllerBase
    {
        private readonly ITemplateFolderRepository _templateFolderRepository;
        private readonly ICurrentUserRepository _currentUserRepository;

        public TemplateFoldersController(ITemplateFolderRepository templateFolderRepository, ICurrentUserRepository currentUserRepository)
        {
            _templateFolderRepository = templateFolderRepository;
            _currentUserRepository = currentUserRepository;
        }

        // GET: api/TemplateFolders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TemplateFolderResponse>>> GetTemplateFolders()
        {
            var userId = _currentUserRepository.UserId ?? "System";
            var folders = await _templateFolderRepository.GetAllTemplateFoldersByUserIdAsync(userId);
            return Ok(folders);
        }

        // GET: api/TemplateFolders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TemplateFolderResponse>> GetTemplateFolder(int id)
        {
            var folder = await _templateFolderRepository.GetTemplateFolderByIdAsync(id);

            if (folder == null)
            {
                return NotFound();
            }

            return Ok(folder);
        }

        // POST: api/TemplateFolders
        [HttpPost]
        public async Task<ActionResult<TemplateFolderResponse>> PostTemplateFolder(CreateTemplateFolderRequest request)
        {
            var userId = _currentUserRepository.UserId ?? "System";
            var userName = _currentUserRepository.UserName ?? "System";

            var folder = await _templateFolderRepository.CreateTemplateFolderAsync(request, userName, userId);

            return CreatedAtAction(nameof(GetTemplateFolder), new { id = folder.Id }, folder);
        }

        // PUT: api/TemplateFolders/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTemplateFolder(int id, UpdateTemplateFolderRequest request)
        {
            var userId = _currentUserRepository.UserId ?? "System";
            var userName = _currentUserRepository.UserName ?? "System";
            var updated = await _templateFolderRepository.UpdateTemplateFolderAsync(id, request, userName);
            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/TemplateFolders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTemplateFolder(int id)
        {
            var userName = User.Identity?.Name ?? "System";
            var deleted = await _templateFolderRepository.DeleteTemplateFolderAsync(id, userName);

            if (!deleted)
            {
                return NotFound(new { message = "Folder not found or has subfolders" });
            }

            return NoContent();
        }
    }
}