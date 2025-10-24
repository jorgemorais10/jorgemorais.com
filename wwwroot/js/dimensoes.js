// Dimensões JavaScript - Handles expandable dimension rows

document.addEventListener('DOMContentLoaded', function() {
    initializeDimensoesExpandCollapse();
    initializeDimensoesAccordionIcons();
});

// Initialize expand/collapse buttons for dimensões
function initializeDimensoesExpandCollapse() {
    const expandAllBtn = document.getElementById('expandAllDimensoes');
    const collapseAllBtn = document.getElementById('collapseAllDimensoes');

    if (expandAllBtn) {
        expandAllBtn.addEventListener('click', function() {
            const allCollapses = document.querySelectorAll('#dimensoesAccordion .accordion-collapse');
            allCollapses.forEach(collapse => {
                const bsCollapse = new bootstrap.Collapse(collapse, {
                    toggle: false
                });
                bsCollapse.show();
            });
        });
    }

    if (collapseAllBtn) {
        collapseAllBtn.addEventListener('click', function() {
            const allCollapses = document.querySelectorAll('#dimensoesAccordion .accordion-collapse');
            allCollapses.forEach(collapse => {
                const bsCollapse = new bootstrap.Collapse(collapse, {
                    toggle: false
                });
                bsCollapse.hide();
            });
        });
    }
}

// Animate accordion icons for dimensões
function initializeDimensoesAccordionIcons() {
    const accordionButtons = document.querySelectorAll('#dimensoesAccordion [data-bs-toggle="collapse"]');

    accordionButtons.forEach(button => {
        const targetId = button.getAttribute('data-bs-target');
        const target = document.querySelector(targetId);

        if (target) {
            target.addEventListener('show.bs.collapse', function() {
                const icon = button.querySelector('.accordion-icon-small');
                if (icon) {
                    icon.style.transform = 'rotate(180deg)';
                }
                button.classList.remove('collapsed');
            });

            target.addEventListener('hide.bs.collapse', function() {
                const icon = button.querySelector('.accordion-icon-small');
                if (icon) {
                    icon.style.transform = 'rotate(0deg)';
                }
                button.classList.add('collapsed');
            });
        }
    });
}
