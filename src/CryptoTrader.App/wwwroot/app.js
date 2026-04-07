let chartInstance = null;

function initChart() {
    const ctx = document.getElementById('priceChart').getContext('2d');
    
    // Gradient fill beneath the line curve
    const gradient = ctx.createLinearGradient(0, 0, 0, 400);
    gradient.addColorStop(0, 'rgba(59, 130, 246, 0.5)');
    gradient.addColorStop(1, 'rgba(59, 130, 246, 0.0)');

    chartInstance = new Chart(ctx, {
        type: 'line',
        data: {
            labels: [],
            datasets: [{
                label: 'Price (USD)',
                data: [],
                borderColor: '#3b82f6',
                borderWidth: 3,
                backgroundColor: gradient,
                fill: true,
                tension: 0.4, // Smooth curvy line
                pointRadius: 0,
                pointHitRadius: 10
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            animation: {
                duration: 0 // Disable native animation to prevent flash on fast re-renders
            },
            plugins: {
                legend: { display: false }
            },
            scales: {
                x: {
                    grid: { display: false, color: 'rgba(255,255,255,0.05)' },
                    ticks: { color: '#94a3b8' }
                },
                y: {
                    grid: { color: 'rgba(255,255,255,0.05)' },
                    ticks: { 
                        color: '#94a3b8',
                        callback: function(value) { return '$' + value; }
                    }
                }
            }
        }
    });
}

function updateUI(state) {
    if (state.statusError) {
        document.getElementById('bot-status-text').textContent = state.statusError;
        document.getElementById('bot-status-dot').style.background = '#ef4444';
        document.getElementById('bot-status-dot').style.boxShadow = '0 0 10px #ef4444';
    } else {
        document.getElementById('bot-status-text').textContent = 'Live Computing';
        document.getElementById('bot-status-dot').style.background = '#10b981';
        document.getElementById('bot-status-dot').style.boxShadow = '0 0 10px #10b981';
    }

    document.getElementById('pair-label').textContent = state.tradingPair;
    document.getElementById('current-price').textContent = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(state.currentPrice);
    
    // Net profit formatting with color injection
    const profitEl = document.getElementById('profit-val');
    profitEl.textContent = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(state.netProfit);
    profitEl.className = state.netProfit >= 0 ? 'positive' : 'negative';

    document.getElementById('portfolio-val').textContent = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(state.portfolioValue);
    
    const positionEl = document.getElementById('position-val');
    positionEl.textContent = state.isInPosition ? 'Holding Asset' : 'Flat (USD)';
    positionEl.className = state.isInPosition ? 'positive' : 'negative';

    // Indicators
    document.getElementById('rsi-val').textContent = state.rsi14.toFixed(2);
    document.getElementById('macd-val').textContent = state.macdLine.toFixed(2);
    document.getElementById('signal-val').textContent = state.macdSignal.toFixed(2);
    document.getElementById('hist-val').textContent = state.macdHistogram.toFixed(2);

    // Action Center Updates
    const actionEl = document.getElementById('last-action');
    actionEl.textContent = state.lastAction;
    
    if (state.lastAction.includes('BUY')) {
        actionEl.style.background = 'rgba(16, 185, 129, 0.2)';
        actionEl.style.borderColor = 'rgba(16, 185, 129, 0.5)';
        actionEl.style.color = '#10b981';
    } else if (state.lastAction.includes('SELL')) {
        actionEl.style.background = 'rgba(239, 68, 68, 0.2)';
        actionEl.style.borderColor = 'rgba(239, 68, 68, 0.5)';
        actionEl.style.color = '#ef4444';
    } else if (state.lastAction.includes('SKIPPED')) {
        actionEl.style.background = 'rgba(245, 158, 11, 0.2)';
        actionEl.style.borderColor = 'rgba(245, 158, 11, 0.5)';
        actionEl.style.color = '#f59e0b';
    } else {
        actionEl.style.background = 'rgba(59, 130, 246, 0.2)';
        actionEl.style.borderColor = 'rgba(59, 130, 246, 0.5)';
        actionEl.style.color = '#3b82f6';
    }

    document.getElementById('trade-count').textContent = state.loggedTradesCount;

    // Update Chart with last 60 ticks
    if (state.recentPrices && state.recentPrices.length > 0) {
        chartInstance.data.labels = state.recentTimestamps;
        chartInstance.data.datasets[0].data = state.recentPrices;
        chartInstance.update();
    }
}

async function fetchState() {
    try {
        const response = await fetch('/api/status');
        if (response.ok) {
            const data = await response.json();
            updateUI(data);
        }
    } catch (err) {
        console.error("Failed to fetch bot state", err);
        document.getElementById('bot-status-text').textContent = 'Server Offline';
        document.getElementById('bot-status-dot').style.background = '#ef4444';
    }
}

// Initialization
document.addEventListener('DOMContentLoaded', () => {
    initChart();
    fetchState();
    setInterval(fetchState, 2000); // Poll API every 2 seconds matching ASP.NET host speeds
});
