




const wsUri = "wss://localhost:7208/WEBAPI/WEBSOCKET/WebConReq";
const websocket = new WebSocket(wsUri);
const MYUUID = '';
const IPLOGIN = 'IPLOGIN';
const EOMHEADER = '|HEAD|';
const UUIDHEADER = 'UUID'
init(2, 2);


websocket.onopen = (e) => {
    websocket.send('LISTPAGE');
};

websocket.onclose = (e) => {

    console.log('onclose');
};

websocket.onmessage = (e) => {
    websocket.send('ack');
    let data = e.data;
    if (data.includes(IPLOGIN) == true) {
        let ips = getDataFromPacket(data, IPLOGIN, EOMHEADER);
        let arrips = ips.split(',');
        CreateCard(arrips);
    }
    else if (data.includes(UUIDHEADER)) {
        let data = e.data;
        MYUUID = getDataFromPacket(data, UUIDHEADER, EOMHEADER);
    }
};

function getDataFromPacket(data, HEADER, EOM) {

    let DATAEOM = data.substring(HEADER.length);
    let EomCnt = DATAEOM.indexOf(EOM);
    let message = DATAEOM.substring(0, EomCnt);
    return message
}

websocket.onerror = (e) => {
    console.log('onerror');
};


function CreateCard(ips) {
    let redraw = document.getElementById('machine');
    let container = document.getElementById('CardContainer');
    if (container != null)
    {

        while (container.childNodes.length != 0) {
            container.firstChild.remove();
        }

        ips.forEach(ip => {
            console.log(ip);
            if (ip == '') {
                return;
            }

            let cCol = document.createElement('div');
            cCol.className += 'col';
            cCol.id += ip;
            container.appendChild(cCol);

            let card = document.createElement('div');
            card.className += 'card';
            cCol.appendChild(card);

            let cardH = document.createElement('div');
            cardH.className += 'card-header';
            cardH.textContent += ip;
            card.appendChild(cardH);

            let cardB = document.createElement('div');
            cardB.className += 'card-body';
           
            card.appendChild(cardB);

            let anchor = document.createElement('a');
            anchor.className += 'btn btn-lg btn-primary'
            anchor.textContent = '장비감시';
            anchor.href = `/MachinMonitor/Index?ipPort=${ip}`;

            cardB.appendChild(anchor);


        });

       
            
        }
    }


function init(col, row) {
    let container = document.createElement('div');
    container.setAttribute('id', 'CardContainer');
    container.className += `row row-cols-${col} row-cols-md-${row}`;
    let rootdiv = document.getElementById('machine');
    rootdiv.appendChild(container);

}