

const wsUri = "wss://localhost:7208/WEBAPI/WEBSOCKET/WebConReq";

const MYUUID = '';
const IPLOGIN = 'IPLOGIN';
const EOMHEADER = '|HEAD|';
const UUIDHEADER = 'UUID';
const MONITORHEADER = 'MONITORD'


const chartData = [
    
];

var myChart = null;


const websocket = new WebSocket(wsUri);

websocket.onclose = (e) => {

    console.log(e.data);
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
    else {
       

        if (myChart == null) {
            parseMonitorData(e.data, EOMHEADER, EOMHEADER, true);
            myChart = new Chart(
                document.getElementById('acquisitions'),
                {
                    type: 'line',
                    data: {
                        labels: chartData.map(row => row.date),
                        datasets: [
                            {
                                label: 'CPU 사용량 감시',
                                data: chartData.map(row => row.usage)
                            }
                        ]
                    },
                    options: {
                        scales: {
                            x: {
                                type: 'time',
                                time: {
                                    unit: 'minute',
                                    displayformat: {
                                        day: 'mm:ss'
                                    }
                                }
                            }
                        }
                    }



                }
            );
        }
        else {
            parseMonitorData(e.data, EOMHEADER, EOMHEADER, false);
        }
     
        
    }
};

websocket.onerror = (e) => {
    console.log('onerror');
};


function getDataFromPacket(data, HEADER, EOM) {

    let DATAEOM = data.substring(HEADER.length);
    let EomCnt = DATAEOM.indexOf(EOM);
    let message = DATAEOM.substring(0, EomCnt);
    return message
}

function parseMonitorData(data, HEAD, EOM , init) {
    let catecnt = data.indexOf(',');
    let category = data.substring(0, catecnt);

    let headCnt = data.indexOf(HEAD);
    let colstr = data.substring(0, headCnt);

    let col = colstr.split(',');
    col.shift();


    let dataArr = data.substring(headCnt + HEAD.length + 1, data.length + 1 - (HEAD.length + EOM.length));

    let mData = dataArr.split(',');
    let pusage = mData[1];
    let time = mData[2];
    console.log(pusage);
    console.log(time);

    if (init) {
        chartData.push({ date: Date.parse(time), usage: Number(pusage) });
    }
    else {
        addData(myChart, Date.parse(time), Number(pusage));
    }
    return category;
}


function addData(chart, label, data) {
    chart.data.labels.push(label);
    chart.data.datasets.forEach((dataset) => {
        dataset.data.push(data);
    });
    chart.update();
}