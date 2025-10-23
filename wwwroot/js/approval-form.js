// Approval Form JavaScript - Handles dynamic dimensões and file uploads

let dimensaoIndex = 0;

document.addEventListener('DOMContentLoaded', function() {
    initializeDimensoes();
    initializeFileUpload();
    initializeDeleteAnexo();
    initializeFormValidation();
});

// Initialize dimensões functionality
function initializeDimensoes() {
    const addBtn = document.getElementById('addDimensaoBtn');
    if (addBtn) {
        addBtn.addEventListener('click', addDimensaoRow);
    }

    // Set initial index based on existing dimensões
    const existingDimensoes = document.querySelectorAll('.dimensao-row');
    dimensaoIndex = existingDimensoes.length;

    // Add remove handlers to existing dimensões
    document.querySelectorAll('.removeDimensaoBtn').forEach(btn => {
        btn.addEventListener('click', function() {
            removeDimensaoRow(this);
        });
    });
}

// Add new dimensão row
function addDimensaoRow() {
    const container = document.getElementById('dimensoesContainer');
    const valorTotal = parseFloat(document.getElementById('valorTotal').value) || 0;

    const rowHtml = `
        <div class="dimensao-row card mb-2" data-index="${dimensaoIndex}">
            <div class="card-body">
                <div class="row g-2">
                    <input type="hidden" name="Dimensoes[${dimensaoIndex}].Linha" value="${dimensaoIndex + 1}" />

                    <div class="col-md-3">
                        <label class="form-label">Tipo</label>
                        <select name="Dimensoes[${dimensaoIndex}].Tipo" class="form-select form-select-sm" required>
                            <option value="">Selecione...</option>
                            <option value="CentroCusto">Centro de Custo</option>
                            <option value="Cliente">Cliente</option>
                            <option value="Produto">Produto</option>
                            <option value="Marca">Marca</option>
                            <option value="Mercado">Mercado</option>
                            <option value="Projeto">Projeto</option>
                        </select>
                    </div>

                    <div class="col-md-2">
                        <label class="form-label">Código</label>
                        <input type="text"
                               name="Dimensoes[${dimensaoIndex}].CodDimensao"
                               class="form-control form-control-sm"
                               placeholder="Código" />
                    </div>

                    <div class="col-md-3">
                        <label class="form-label">Nome</label>
                        <input type="text"
                               name="Dimensoes[${dimensaoIndex}].NomeDimensao"
                               class="form-control form-control-sm"
                               placeholder="Nome da dimensão" />
                    </div>

                    <div class="col-md-2">
                        <label class="form-label">Percentagem (%)</label>
                        <input type="number"
                               name="Dimensoes[${dimensaoIndex}].Percentagem"
                               class="form-control form-control-sm dimensao-percentagem"
                               step="0.01"
                               min="0"
                               max="100"
                               placeholder="0.00"
                               data-index="${dimensaoIndex}" />
                    </div>

                    <div class="col-md-2">
                        <label class="form-label">Valor</label>
                        <div class="input-group input-group-sm">
                            <input type="number"
                                   name="Dimensoes[${dimensaoIndex}].Valor"
                                   class="form-control dimensao-valor"
                                   step="0.01"
                                   min="0"
                                   placeholder="0.00"
                                   data-index="${dimensaoIndex}"
                                   readonly />
                            <button type="button" class="btn btn-danger removeDimensaoBtn">
                                <i class="bi bi-trash"></i>
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `;

    container.insertAdjacentHTML('beforeend', rowHtml);

    // Add event listeners to the new row
    const newRow = container.lastElementChild;
    newRow.querySelector('.removeDimensaoBtn').addEventListener('click', function() {
        removeDimensaoRow(this);
    });

    // Add percentage change listener
    const percentInput = newRow.querySelector('.dimensao-percentagem');
    percentInput.addEventListener('input', function() {
        calculateDimensaoValor(this);
    });

    dimensaoIndex++;

    showToast('Dimensão adicionada', 'success');
}

// Remove dimensão row
function removeDimensaoRow(button) {
    const row = button.closest('.dimensao-row');
    row.style.transition = 'opacity 0.3s ease';
    row.style.opacity = '0';

    setTimeout(() => {
        row.remove();
        reindexDimensoes();
        showToast('Dimensão removida', 'info');
    }, 300);
}

// Reindex dimensões after removal
function reindexDimensoes() {
    const rows = document.querySelectorAll('.dimensao-row');
    rows.forEach((row, index) => {
        row.setAttribute('data-index', index);

        // Update all input names and IDs
        const inputs = row.querySelectorAll('input, select');
        inputs.forEach(input => {
            const name = input.getAttribute('name');
            if (name) {
                const newName = name.replace(/\[\d+\]/, `[${index}]`);
                input.setAttribute('name', newName);
            }

            if (input.hasAttribute('data-index')) {
                input.setAttribute('data-index', index);
            }
        });

        // Update linha value
        const linhaInput = row.querySelector('input[name*=".Linha"]');
        if (linhaInput) {
            linhaInput.value = index + 1;
        }
    });

    dimensaoIndex = rows.length;
}

// Calculate dimensão valor based on percentage
function calculateDimensaoValor(percentInput) {
    const index = percentInput.getAttribute('data-index');
    const valorTotal = parseFloat(document.getElementById('valorTotal').value) || 0;
    const percentage = parseFloat(percentInput.value) || 0;

    const valorInput = document.querySelector(`input[name="Dimensoes[${index}].Valor"]`);
    if (valorInput) {
        const valor = (valorTotal * percentage) / 100;
        valorInput.value = valor.toFixed(2);
    }
}

// Update all dimensão valores when total value changes
document.addEventListener('DOMContentLoaded', function() {
    const valorTotalInput = document.getElementById('valorTotal');
    if (valorTotalInput) {
        valorTotalInput.addEventListener('input', function() {
            document.querySelectorAll('.dimensao-percentagem').forEach(input => {
                calculateDimensaoValor(input);
            });
        });
    }
});

// Initialize file upload functionality
function initializeFileUpload() {
    const fileInput = document.getElementById('files');
    const filesList = document.getElementById('filesList');

    if (fileInput && filesList) {
        fileInput.addEventListener('change', function() {
            filesList.innerHTML = '';

            if (this.files.length > 0) {
                const ul = document.createElement('ul');
                ul.className = 'list-group';

                Array.from(this.files).forEach(file => {
                    const li = document.createElement('li');
                    li.className = 'list-group-item d-flex justify-content-between align-items-center';
                    li.innerHTML = `
                        <div>
                            <i class="bi bi-file-earmark"></i>
                            <strong>${file.name}</strong>
                            <small class="text-muted">(${formatFileSize(file.size)})</small>
                        </div>
                        <span class="badge bg-primary rounded-pill">${file.type || 'Desconhecido'}</span>
                    `;
                    ul.appendChild(li);
                });

                filesList.appendChild(ul);
            }
        });
    }
}

// Initialize delete anexo functionality
function initializeDeleteAnexo() {
    document.querySelectorAll('.deleteAnexoBtn').forEach(btn => {
        btn.addEventListener('click', async function() {
            if (!confirm('Tem a certeza que deseja eliminar este anexo?')) {
                return;
            }

            const anexoId = this.getAttribute('data-id');
            const listItem = this.closest('.list-group-item');

            try {
                const response = await fetch(`/ApprovalHub/DeleteAnexo/${anexoId}`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    }
                });

                const result = await response.json();

                if (result.success) {
                    listItem.style.transition = 'opacity 0.3s ease';
                    listItem.style.opacity = '0';
                    setTimeout(() => {
                        listItem.remove();
                    }, 300);
                    showToast('Anexo eliminado com sucesso', 'success');
                } else {
                    showToast('Erro ao eliminar anexo: ' + result.message, 'danger');
                }
            } catch (error) {
                console.error('Error:', error);
                showToast('Erro ao eliminar anexo', 'danger');
            }
        });
    });
}

// Form validation enhancements
function initializeFormValidation() {
    const form = document.getElementById('approvalForm');
    if (form) {
        form.addEventListener('submit', function(e) {
            // Validate dimensões total percentage
            const percentInputs = document.querySelectorAll('.dimensao-percentagem');
            let totalPercent = 0;

            percentInputs.forEach(input => {
                totalPercent += parseFloat(input.value) || 0;
            });

            if (percentInputs.length > 0 && Math.abs(totalPercent - 100) > 0.01) {
                if (!confirm(`A soma das percentagens é ${totalPercent.toFixed(2)}%. Deseja continuar mesmo assim?`)) {
                    e.preventDefault();
                    return false;
                }
            }
        });
    }
}

// Utility function to format file size
function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';

    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));

    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
}

// Utility function to show toast (defined in site.js but repeated here for standalone usage)
function showToast(message, type = 'info') {
    const toastContainer = document.querySelector('.toast-container') || createToastContainer();

    const toast = document.createElement('div');
    toast.className = `toast align-items-center text-white bg-${type} border-0`;
    toast.setAttribute('role', 'alert');
    toast.setAttribute('aria-live', 'assertive');
    toast.setAttribute('aria-atomic', 'true');

    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                ${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
        </div>
    `;

    toastContainer.appendChild(toast);
    const bsToast = new bootstrap.Toast(toast);
    bsToast.show();

    toast.addEventListener('hidden.bs.toast', function() {
        toast.remove();
    });
}

function createToastContainer() {
    const container = document.createElement('div');
    container.className = 'toast-container position-fixed top-0 end-0 p-3';
    container.style.zIndex = '11';
    document.body.appendChild(container);
    return container;
}
