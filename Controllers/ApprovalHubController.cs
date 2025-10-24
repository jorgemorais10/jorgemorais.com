using hub.Models;
using Hub.Services;
using Hub.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Hub.Controllers;

public class ApprovalHubController : Controller
{
    private readonly IApprovalHubService _approvalService;
    private readonly ILogger<ApprovalHubController> _logger;

    public ApprovalHubController(
        IApprovalHubService approvalService,
        ILogger<ApprovalHubController> logger)
    {
        _approvalService = approvalService;
        _logger = logger;
    }

    // GET: ApprovalHub
    public async Task<IActionResult> Index()
    {
        try
        {
            var approvals = await _approvalService.GetAllApprovalHubsAsync();
            var viewModel = CreateGroupedViewModel(approvals);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar lista de aprovações");
            TempData["Error"] = "Erro ao carregar lista de aprovações: " + ex.Message;
            return View(new ApprovalHubIndexViewModel());
        }
    }

    private ApprovalHubIndexViewModel CreateGroupedViewModel(
        List<ApprovalHub> approvals,
        string? fornecedor = null,
        string? estado = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null)
    {
        var grupos = approvals
            .GroupBy(a => a.Aprovador ?? "Sem Aprovador")
            .Select(g => new ApprovalHubGroupedViewModel
            {
                Aprovador = g.Key,
                Pedidos = g.OrderByDescending(p => p.CriadoEm).ToList()
            })
            .OrderBy(g => g.Aprovador)
            .ToList();

        return new ApprovalHubIndexViewModel
        {
            GruposPorAprovador = grupos,
            Fornecedor = fornecedor,
            Estado = estado,
            DataInicio = dataInicio,
            DataFim = dataFim
        };
    }

    // GET: ApprovalHub/Details/5
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var approval = await _approvalService.GetApprovalHubByIdAsync(id);
            if (approval == null)
            {
                return NotFound();
            }

            // Load related data
            approval.Dimensoes = await _approvalService.GetDimensoesByApprovalHubIdAsync(id);
            approval.Anexos = await _approvalService.GetAnexosByApprovalHubIdAsync(id);

            return View(approval);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar detalhes da aprovação {Id}", id);
            TempData["Error"] = "Erro ao carregar detalhes: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: ApprovalHub/Create
    public IActionResult Create()
    {
        var model = new ApprovalHub
        {
            DataDocumento = DateTime.Today,
            Estado = "Pendente",
            Dimensoes = new List<IdsDimensao>(),
            Anexos = new List<Anexo>()
        };
        return View(model);
    }

    // POST: ApprovalHub/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ApprovalHub model, List<IFormFile>? files)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Create approval hub
            var id = await _approvalService.CreateApprovalHubAsync(model);

            if (id > 0)
            {
                // Save dimensões if any
                if (model.Dimensoes != null && model.Dimensoes.Any())
                {
                    var linha = 1;
                    foreach (var dimensao in model.Dimensoes)
                    {
                        dimensao.IdApprovalHub = id;
                        dimensao.Linha = linha++;
                        await _approvalService.CreateDimensaoAsync(dimensao);
                    }
                }

                // Save files if any
                if (files != null && files.Any())
                {
                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            using var ms = new MemoryStream();
                            await file.CopyToAsync(ms);

                            var anexo = new Anexo
                            {
                                IdApprovalHub = id,
                                NomeFicheiro = file.FileName,
                                ContentType = file.ContentType,
                                TamanhoBytes = file.Length,
                                Conteudo = ms.ToArray()
                            };

                            await _approvalService.CreateAnexoAsync(anexo);
                        }
                    }
                }

                TempData["Success"] = "Pedido de aprovação criado com sucesso!";
                return RedirectToAction(nameof(Details), new { id });
            }

            ModelState.AddModelError("", "Erro ao criar pedido de aprovação");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar aprovação");
            ModelState.AddModelError("", "Erro ao criar pedido: " + ex.Message);
            return View(model);
        }
    }

    // GET: ApprovalHub/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var approval = await _approvalService.GetApprovalHubByIdAsync(id);
            if (approval == null)
            {
                return NotFound();
            }

            // Load related data
            approval.Dimensoes = await _approvalService.GetDimensoesByApprovalHubIdAsync(id);
            approval.Anexos = await _approvalService.GetAnexosByApprovalHubIdAsync(id);

            return View(approval);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar aprovação para edição {Id}", id);
            TempData["Error"] = "Erro ao carregar: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: ApprovalHub/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ApprovalHub model, List<IFormFile>? files)
    {
        if (id != model.IdApprovalHub)
        {
            return NotFound();
        }

        try
        {
            if (!ModelState.IsValid)
            {
                model.Dimensoes = await _approvalService.GetDimensoesByApprovalHubIdAsync(id);
                model.Anexos = await _approvalService.GetAnexosByApprovalHubIdAsync(id);
                return View(model);
            }

            var success = await _approvalService.UpdateApprovalHubAsync(model);

            if (success)
            {
                // Update dimensões (delete old and create new)
                await _approvalService.DeleteDimensoesByApprovalHubIdAsync(id);

                if (model.Dimensoes != null && model.Dimensoes.Any())
                {
                    var linha = 1;
                    foreach (var dimensao in model.Dimensoes)
                    {
                        dimensao.IdApprovalHub = id;
                        dimensao.Linha = linha++;
                        await _approvalService.CreateDimensaoAsync(dimensao);
                    }
                }

                // Add new files if any
                if (files != null && files.Any())
                {
                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            using var ms = new MemoryStream();
                            await file.CopyToAsync(ms);

                            var anexo = new Anexo
                            {
                                IdApprovalHub = id,
                                NomeFicheiro = file.FileName,
                                ContentType = file.ContentType,
                                TamanhoBytes = file.Length,
                                Conteudo = ms.ToArray()
                            };

                            await _approvalService.CreateAnexoAsync(anexo);
                        }
                    }
                }

                TempData["Success"] = "Pedido de aprovação atualizado com sucesso!";
                return RedirectToAction(nameof(Details), new { id });
            }

            ModelState.AddModelError("", "Erro ao atualizar pedido de aprovação");
            model.Dimensoes = await _approvalService.GetDimensoesByApprovalHubIdAsync(id);
            model.Anexos = await _approvalService.GetAnexosByApprovalHubIdAsync(id);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar aprovação {Id}", id);
            ModelState.AddModelError("", "Erro ao atualizar: " + ex.Message);
            model.Dimensoes = await _approvalService.GetDimensoesByApprovalHubIdAsync(id);
            model.Anexos = await _approvalService.GetAnexosByApprovalHubIdAsync(id);
            return View(model);
        }
    }

    // GET: ApprovalHub/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var approval = await _approvalService.GetApprovalHubByIdAsync(id);
            if (approval == null)
            {
                return NotFound();
            }

            return View(approval);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar aprovação para exclusão {Id}", id);
            TempData["Error"] = "Erro ao carregar: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: ApprovalHub/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var success = await _approvalService.DeleteApprovalHubAsync(id);

            if (success)
            {
                TempData["Success"] = "Pedido de aprovação eliminado com sucesso!";
            }
            else
            {
                TempData["Error"] = "Erro ao eliminar pedido de aprovação";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao eliminar aprovação {Id}", id);
            TempData["Error"] = "Erro ao eliminar: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: ApprovalHub/UpdateEstado
    [HttpPost]
    public async Task<IActionResult> UpdateEstado(int id, string estado)
    {
        try
        {
            var success = await _approvalService.UpdateEstadoAsync(id, estado);

            if (success)
            {
                return Json(new { success = true, message = "Estado atualizado com sucesso!" });
            }

            return Json(new { success = false, message = "Erro ao atualizar estado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar estado da aprovação {Id}", id);
            return Json(new { success = false, message = ex.Message });
        }
    }

    // GET: ApprovalHub/Search
    public async Task<IActionResult> Search(string? fornecedor, string? estado, DateTime? dataInicio, DateTime? dataFim)
    {
        try
        {
            var approvals = await _approvalService.SearchApprovalHubsAsync(fornecedor, estado, dataInicio, dataFim);
            var viewModel = CreateGroupedViewModel(approvals, fornecedor, estado, dataInicio, dataFim);

            return View("Index", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao pesquisar aprovações");
            TempData["Error"] = "Erro ao pesquisar: " + ex.Message;
            return View("Index", new ApprovalHubIndexViewModel());
        }
    }

    // GET: ApprovalHub/DownloadAnexo/5
    public async Task<IActionResult> DownloadAnexo(int id)
    {
        try
        {
            var anexo = await _approvalService.GetAnexoByIdAsync(id);

            if (anexo == null || anexo.Conteudo == null)
            {
                return NotFound();
            }

            return File(anexo.Conteudo, anexo.ContentType ?? "application/octet-stream", anexo.NomeFicheiro);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer download do anexo {Id}", id);
            TempData["Error"] = "Erro ao fazer download: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: ApprovalHub/DeleteAnexo/5
    [HttpPost]
    public async Task<IActionResult> DeleteAnexo(int id)
    {
        try
        {
            var success = await _approvalService.DeleteAnexoAsync(id);

            if (success)
            {
                return Json(new { success = true, message = "Anexo eliminado com sucesso!" });
            }

            return Json(new { success = false, message = "Erro ao eliminar anexo" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao eliminar anexo {Id}", id);
            return Json(new { success = false, message = ex.Message });
        }
    }

    // API Methods for AJAX
    [HttpGet]
    public async Task<IActionResult> GetFornecedores()
    {
        try
        {
            var fornecedores = await _approvalService.GetDistinctFornecedoresAsync();
            return Json(fornecedores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter fornecedores");
            return Json(new List<string>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetEstados()
    {
        try
        {
            var estados = await _approvalService.GetDistinctEstadosAsync();
            return Json(estados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estados");
            return Json(new List<string>());
        }
    }
}
