// Approval Index JavaScript - Handles expand/collapse and animations

document.addEventListener('DOMContentLoaded', function() {
    initializeExpandCollapse();
    initializeAccordionIcons();
    initializeStatistics();
});

// Initialize expand/collapse all buttons
function initializeExpandCollapse() {
    const expandAllBtn = document.getElementById('expandAllBtn');
    const collapseAllBtn = document.getElementById('collapseAllBtn');

    if (expandAllBtn) {
        expandAllBtn.addEventListener('click', function() {
            const allCollapses = document.querySelectorAll('.accordion-collapse');
            allCollapses.forEach(collapse => {
                const bsCollapse = new bootstrap.Collapse(collapse, {
                    toggle: false
                });
                bsCollapse.show();
            });

            showToast('Todos os grupos expandidos', 'info');
        });
    }

    if (collapseAllBtn) {
        collapseAllBtn.addEventListener('click', function() {
            const allCollapses = document.querySelectorAll('.accordion-collapse');
            allCollapses.forEach(collapse => {
                const bsCollapse = new bootstrap.Collapse(collapse, {
                    toggle: false
                });
                bsCollapse.hide();
            });

            showToast('Todos os grupos colapsados', 'info');
        });
    }
}

// Animate accordion icons on expand/collapse
function initializeAccordionIcons() {
    const accordionButtons = document.querySelectorAll('[data-bs-toggle="collapse"]');

    accordionButtons.forEach(button => {
        const targetId = button.getAttribute('data-bs-target');
        const target = document.querySelector(targetId);

        if (target) {
            target.addEventListener('show.bs.collapse', function() {
                const icon = button.querySelector('.accordion-icon');
                if (icon) {
                    icon.style.transform = 'rotate(180deg)';
                }
                button.classList.remove('collapsed');
            });

            target.addEventListener('hide.bs.collapse', function() {
                const icon = button.querySelector('.accordion-icon');
                if (icon) {
                    icon.style.transform = 'rotate(0deg)';
                }
                button.classList.add('collapsed');
            });
        }
    });
}

// Initialize statistics with animations
function initializeStatistics() {
    // Observe summary cards for fade-in animation
    const observerOptions = {
        root: null,
        threshold: 0.1,
        rootMargin: '0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('fade-in');
                animateValue(entry.target);
            }
        });
    }, observerOptions);

    // Observe all cards
    const cards = document.querySelectorAll('.card');
    cards.forEach(card => {
        observer.observe(card);
    });
}

// Animate number counting
function animateValue(element) {
    const valueElements = element.querySelectorAll('h2');

    valueElements.forEach(el => {
        const text = el.textContent.trim();

        // Check if it's a number (not currency)
        const match = text.match(/^(\d+)$/);
        if (match) {
            const target = parseInt(match[1]);
            animateCounter(el, 0, target, 1000);
        }
    });
}

function animateCounter(element, start, end, duration) {
    const range = end - start;
    const increment = range / (duration / 16); // 60fps
    let current = start;

    const timer = setInterval(() => {
        current += increment;
        if (current >= end) {
            current = end;
            clearInterval(timer);
        }
        element.textContent = Math.floor(current);
    }, 16);
}

// Highlight search results if filtering
function highlightSearchResults() {
    const urlParams = new URLSearchParams(window.location.search);
    const hasFilters = urlParams.has('fornecedor') || urlParams.has('estado') ||
                      urlParams.has('dataInicio') || urlParams.has('dataFim');

    if (hasFilters) {
        const accordion = document.getElementById('aprovadoresAccordion');
        if (accordion) {
            accordion.classList.add('search-results');
        }
    }
}

// Row click to view details (optional)
function initializeRowClick() {
    const rows = document.querySelectorAll('.approval-row');

    rows.forEach(row => {
        row.style.cursor = 'pointer';

        row.addEventListener('click', function(e) {
            // Don't trigger if clicking on buttons
            if (e.target.closest('.btn-group')) {
                return;
            }

            // Find the details link and navigate
            const detailsLink = row.querySelector('[title="Ver Detalhes"]');
            if (detailsLink) {
                window.location.href = detailsLink.href;
            }
        });

        // Add hover effect
        row.addEventListener('mouseenter', function() {
            this.style.backgroundColor = 'rgba(13, 110, 253, 0.05)';
        });

        row.addEventListener('mouseleave', function() {
            this.style.backgroundColor = '';
        });
    });
}

// Initialize row click after accordion expands
document.addEventListener('shown.bs.collapse', function() {
    initializeRowClick();
});

// Keyboard shortcuts
document.addEventListener('keydown', function(e) {
    // Ctrl/Cmd + E: Expand all
    if ((e.ctrlKey || e.metaKey) && e.key === 'e') {
        e.preventDefault();
        document.getElementById('expandAllBtn')?.click();
    }

    // Ctrl/Cmd + R: Collapse all
    if ((e.ctrlKey || e.metaKey) && e.key === 'r') {
        e.preventDefault();
        document.getElementById('collapseAllBtn')?.click();
    }

    // Ctrl/Cmd + N: New approval
    if ((e.ctrlKey || e.metaKey) && e.key === 'n') {
        e.preventDefault();
        const createBtn = document.querySelector('[asp-action="Create"]');
        if (createBtn) {
            window.location.href = createBtn.href;
        }
    }
});

// Save scroll position on page unload
window.addEventListener('beforeunload', function() {
    sessionStorage.setItem('scrollPosition', window.scrollY);
});

// Restore scroll position on page load
window.addEventListener('load', function() {
    const scrollPosition = sessionStorage.getItem('scrollPosition');
    if (scrollPosition) {
        window.scrollTo(0, parseInt(scrollPosition));
        sessionStorage.removeItem('scrollPosition');
    }
});

// Export to CSV functionality (bonus feature)
function exportToCSV() {
    const rows = [];
    const headers = ['ID', 'Aprovador', 'Nº Fatura', 'Fornecedor', 'Data', 'Valor', 'Estado'];
    rows.push(headers);

    document.querySelectorAll('.approval-row').forEach(row => {
        const cells = row.querySelectorAll('td');
        const rowData = [];

        // Extract text from cells (excluding action buttons)
        for (let i = 0; i < cells.length - 1; i++) {
            rowData.push(cells[i].textContent.trim().replace(/\n/g, ' '));
        }

        rows.push(rowData);
    });

    // Create CSV content
    const csvContent = rows.map(row => row.join(';')).join('\n');

    // Download
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);

    link.setAttribute('href', url);
    link.setAttribute('download', `pedidos_aprovacao_${new Date().toISOString().split('T')[0]}.csv`);
    link.style.visibility = 'hidden';

    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

// Add export button if needed (can be called from UI)
function addExportButton() {
    const headerRow = document.querySelector('.row.mb-4');
    if (headerRow && !document.getElementById('exportBtn')) {
        const exportBtn = document.createElement('button');
        exportBtn.id = 'exportBtn';
        exportBtn.className = 'btn btn-outline-success';
        exportBtn.innerHTML = '<i class="bi bi-download"></i> Exportar CSV';
        exportBtn.onclick = exportToCSV;

        const colAuto = headerRow.querySelector('.col-auto');
        if (colAuto) {
            colAuto.appendChild(exportBtn);
        }
    }
}

// Optional: Add print functionality
function printGroups() {
    window.print();
}

// Utility: Show toast notification
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
    const bsToast = new bootstrap.Toast(toast, { delay: 3000 });
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

// Initialize on load
highlightSearchResults();

// Show keyboard shortcuts hint
console.log('%c⌨️ Atalhos de Teclado:', 'font-size: 14px; font-weight: bold; color: #0d6efd;');
console.log('Ctrl/Cmd + E: Expandir todos');
console.log('Ctrl/Cmd + R: Colapsar todos');
console.log('Ctrl/Cmd + N: Novo pedido');
