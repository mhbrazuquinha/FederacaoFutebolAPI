document.addEventListener('DOMContentLoaded', async () => {
    // Busca times da API e preenche a tabela
    async function carregarTimes() {
        const response = await fetch('/api/times');  // Substitua pela sua rota real
        const times = await response.json();
        
        const tabela = document.querySelector('#tabelaTimes tbody');
        tabela.innerHTML = times.map(time => `
            <tr>
                <td>${time.id}</td>
                <td>${time.nome}</td>
                <td>${time.cidade}</td>
            </tr>
        `).join('');
    }

    // Chama as funções ao carregar a página
    await carregarTimes();
});