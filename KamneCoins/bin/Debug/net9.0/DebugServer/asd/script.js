async function sendGetRequest() {
    const param = document.getElementById('getParam').value;
    try {
        const response = await fetch(`./server.csref?param=${encodeURIComponent(param)}`);
        const text = await response.text();
        document.getElementById('getResponse').innerText = text;
    } catch (error) {
        document.getElementById('getResponse').innerText = "Ошибка запроса GET";
    }
}

async function sendPostRequest() {
    const data = document.getElementById('postData').value;
    try {
        const response = await fetch('./server.csref', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ data })
        });
        const text = await response.text();
        document.getElementById('postResponse').innerText = text;
    } catch (error) {
        document.getElementById('postResponse').innerText = "Ошибка запроса POST";
    }
}
