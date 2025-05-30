window.renderPriceChart = (data) => {
    console.log("[DEBUG] Iniciando renderPriceChart com dados:", JSON.stringify(data));
    const ctx = document.getElementById('priceChart').getContext('2d');
    if (!ctx) {
        console.error("[ERROR] Elemento canvas 'priceChart' não encontrado.");
        return;
    }

    // Função para gerar cores aleatórias
    const getRandomColor = () => {
        const letters = '0123456789ABCDEF';
        let color = '#';
        for (let i = 0; i < 6; i++) {
            color += letters[Math.floor(Math.random() * 16)];
        }
        return color;
    };

    try {
        new Chart(ctx, {
            type: 'line',
            data: {
                datasets: data.map(store => ({
                    label: store.LojaNome,
                    data: store.Prices.map(p => ({
                        x: new Date(p.Date),
                        y: p.Price
                    })),
                    borderColor: store.LojaNome === "Média" ? 'rgb(75, 192, 192)' : getRandomColor(),
                    fill: false,
                    tension: 0.1
                }))
            },
            options: {
                responsive: true,
                scales: {
                    x: {
                        type: 'time',
                        time: {
                            unit: 'day',
                            tooltipFormat: 'dd/MM/yyyy'
                        },
                        title: {
                            display: true,
                            text: 'Data'
                        }
                    },
                    y: {
                        title: {
                            display: true,
                            text: 'Preço (€)'
                        },
                        beginAtZero: true
                    }
                },
                plugins: {
                    legend: {
                        display: true,
                        position: 'top'
                    }
                }
            }
        });
        console.log("[DEBUG] Gráfico renderizado com sucesso.");
    } catch (error) {
        console.error("[ERROR] Erro ao renderizar o gráfico:", error);
    }
};
