using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Testify.Data;
using Testify.Entities;

namespace Testify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectFoldersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProjectFoldersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ProjectFolders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectFolder>>> GetProjectFolders()
        {
            return await _context.ProjectFolders.ToListAsync();
        }

        // GET: api/ProjectFolders/project/{projectId}
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<IEnumerable<ProjectFolder>>> GetProjectFoldersByProject(int projectId)
        {
            var folders = await _context.ProjectFolders
                .Where(f => f.ProjectId == projectId)
                .OrderBy(f => f.Name)
                .ToListAsync();
            return Ok(folders);
        }

        // GET: api/ProjectFolders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectFolder>> GetProjectFolder(int id)
        {
            var projectFolder = await _context.ProjectFolders.FindAsync(id);

            if (projectFolder == null)
            {
                return NotFound();
            }

            return projectFolder;
        }

        // PUT: api/ProjectFolders/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProjectFolder(int id, ProjectFolder projectFolder)
        {
            if (id != projectFolder.Id)
            {
                return BadRequest();
            }

            _context.Entry(projectFolder).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectFolderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<ProjectFolder>> PostProjectFolder(ProjectFolder projectFolder)
        {
            _context.ProjectFolders.Add(projectFolder);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProjectFolder", new { id = projectFolder.Id }, projectFolder);
        }

        // DELETE: api/ProjectFolders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProjectFolder(int id)
        {
            var projectFolder = await _context.ProjectFolders.FindAsync(id);
            if (projectFolder == null)
            {
                return NotFound();
            }

            _context.ProjectFolders.Remove(projectFolder);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProjectFolderExists(int id)
        {
            return _context.ProjectFolders.Any(e => e.Id == id);
        }
    }
}
