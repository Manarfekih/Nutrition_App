// Chart.js interop helpers for Blazor
window.ChartInterop = {
    charts: {},

    createOrUpdate: function (canvasId, type, labels, datasets, options) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;

        // Destroy existing chart on this canvas if any
        if (this.charts[canvasId]) {
            this.charts[canvasId].destroy();
        }

        const config = {
            type: type,
            data: { labels: labels, datasets: datasets },
            options: options || {}
        };

        this.charts[canvasId] = new Chart(canvas, config);
    },

    destroy: function (canvasId) {
        if (this.charts[canvasId]) {
            this.charts[canvasId].destroy();
            delete this.charts[canvasId];
        }
    }
};
