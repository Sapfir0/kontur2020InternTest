const MAX_LOAD_SHIP = 368;

let portsCoordinates = [];
let homePort = {};
let ship;
let map = [ []];

let lenToPorts = {};

class Ship {
    x = 0;
    y = 0;
    items;

    constructor(gameState) {
        this.refreshShipState(gameState)
    }

    getLocation() {
        const loc = {x:this.x, y:this.y}
        return loc;
    }

    refreshShipState(gameState) {
        this.x = gameState.x;
        this.y = gameState.y;
        this.items = gameState.goods;
    }

    isInTradePort(gameState) {
        const portsArray = gameState.ports.filter(port => port.isHome && this.weAreIn(port));
        return !!portsArray;
    }

    isHomePort() {
        return this.weAreIn(homePort);
    }

    weAreIn(something) {
        return this.x === something.x && this.y === something.y;
    }

    notHaveItems() {
        return ship.items.length > 0
    }

    moveToSouth() {
        return 'S'
    }

    moveToNorth() {
        return 'N'
    }

    moveToEast() {
        return 'E'
    }

    moveToWest() {
        return 'W'
    }

    wait() {
        return 'W'
    }

    needSale(gameState) {
        return ship.isInTradePort(gameState) && ship.weAreIn(findOptimalPort(gameState.ports))
    }

    freeSpaceInShip() {
        return ship.items.reduce((acc, cur) => acc - productVolume[cur.name]*cur.amount, MAX_LOAD_SHIP);
    }

}

class Port {
    id;
    x;
    y;
    constructor(id, x, y) {
        this.id = id;
        this.x = x;
        this.y = y;
    }
}

class HomePort extends Port {

}

class TradingPort extends Port {
    prices;
    constructor(id, x, y, prices) {
        super(id, x, y);
        this.prices = prices;
    }
}

class Maths {
    static distance(obj1, obj2) {
        return Math.abs(obj1.x - obj2.x) + Math.abs(obj1.y - obj2.y);
    }

    static maxWithAmount(priceOnCurrentPort, obj1, obj2) {
        const priceWithAmount = (product) => product && console.log("foo") && (priceOnCurrentPort[product.name]*product.amount);
        const price1 = priceWithAmount(obj1);
        const price2 = priceWithAmount(obj2);
        if (price1 > price2) return obj1; else return obj2;
    }

    static priceWithAmount(product) {
        return product && product.max_price * product.amount;

    }
}

function needLoadProduct(gameState) {
    const freeSpace = ship.freeSpaceInShip();
    const thereLoad = freeSpace < MAX_LOAD_SHIP;
    if (thereLoad) {
        const port = findOptimalPort(gameState.ports);
        //console.log(port)
        const price = port.prices;
        return (freeSpace >= 50) && gameState.goodsInPort.reduce(
            (acc, good) => acc || (price.hasOwnProperty(good.name) && good.volume <= freeSpace),
            false);
    }
    return gameState.goodsInPort.length !== 0;
}


class MapObject {
    x;
    y;
    isHomePort;
    isTradePort;
    constructor(x,y,isHomePort=false, isTradePort=false) {
        this.x = x;
        this.y = y;
        this.isHomePort = isHomePort;
        this.isTradePort = isTradePort;
    }
}



class Node {
    childrens = []
    mapObject;
    parent;
    constructor(mapObject, parent=null) {
        this.mapObject = mapObject;
        this.parent = parent;
    }
}

function matrixArray(rows,columns){
    var arr = [];
    for(var i=0; i<rows; i++){
        arr[i] = [];
        for(var j=0; j<columns; j++){
            arr[i][j] = 0;//вместо i+j+1 пишем любой наполнитель. В простейшем случае - null
        }
    }
    return arr;
}

function createMapObject(symbol, x, y) {
    let mapObject;
    switch (symbol) {
        case "O": {
            mapObject = new MapObject(x,y,false, true)
            break;
        }
        case "H": {
            mapObject = new MapObject(x,y, true)
            break;
        }
        case "~": {
            mapObject = new MapObject(x,y)
            break;
        }
    }
    return mapObject;
}


function parseMap(levelMap) {
    const matrix = levelMap.split('\n');

    for (let x=0; x<matrix.length; x++) {
        matrix[x] = matrix[x].split("")
    }

    const width = matrix.length;
    const height = matrix[0].length

    let matrixAdjasment = matrixArray(width, height);
    //console.log(matrixAdjasment)

    for (let x=1; x<matrix.length-1; x++) {
        for (let y=1; y<matrix[x].length-1; y++) {
            const currentCell = matrix[x][y];
            const leftCell = new MapObject(x-1,y)
            const rightCell = new MapObject(x+1,y);
            const downCell = new MapObject(x, y-1);
            const upCell = new MapObject(x, y+1);
            const neighbours = [leftCell, rightCell, downCell, upCell];
            if (currentCell !== "#") {
                const mapObject = createMapObject(currentCell, x,y)
                let node = new Node(mapObject)

                for (let i=0; i<neighbours.length; i++) {
                    if(matrix[neighbours[i].x][neighbours[i].y] !== "#") {
                        const innerMapObject = createMapObject(matrix[neighbours[i].x][neighbours[i].y], neighbours[i].x, neighbours[i].y)
                        const childrens = new Node(innerMapObject)
                        childrens.parent = node;
                        node.childrens.push(childrens)
                    }
                }
                matrixAdjasment[x][y] = node;
            }

        }
    }
    return matrixAdjasment;
}

let productVolume;

export function startGame(levelMap, gameState) {
    const newmap = parseMap(levelMap);
    //console.log(newmap)
    ship = new Ship(gameState.ship);

    for (let i=0; i<gameState.ports.length; i++) { // дополним наш массив ценами
        const currentPortId = gameState.ports[i].portId;
        gameState.ports[i].prices = gameState.prices.filter(price => price.portId === currentPortId)[0]
    }

    const homePortArray = gameState.ports.filter(port => port.isHome)[0];
    const portsCoordinatesArray = gameState.ports.filter(port => !port.isHome);

    homePort = new HomePort(homePortArray.portId, homePortArray.x, homePortArray.y);
    portsCoordinatesArray.forEach(port =>
        portsCoordinates.push(new TradingPort(port.portId, port.x, port.y, port.prices)))

    productVolume = {};
    gameState.goodsInPort.forEach(good => {
        productVolume[good.name] = good.volume;
    });
}


function isInTradePortFake(gameState) {
    const portsArray = gameState.ports.filter(port => !port.isHome && isEqualPosition(gameState.ship, port));
    return !!portsArray;
}

function isEqualPosition(obj1, obj2) {
    return obj1.x === obj2.x && obj1.y === obj2.y;
}

export function getNextCommand(gameState) {
    let command;
    ship.refreshShipState(gameState.ship);
    if (ship.isHomePort() && needLoadProduct(gameState)) {
        const product = getProductForLoad(gameState);
        command = `LOAD ${product.name} ${product.amount}`
    } else if (isInTradePortFake(gameState) && ship.needSale(gameState)) {
        const product = getProductForSale();
        console.log(isEqualPosition(ship, gameState.ports[1]))
        //console.log(gameState.ports)
        command = `SELL ${product.name} ${product.amount}`
    } else {
        command = goto(gameState);
    }
    console.log(command)
    console.log(ship.getLocation().x, ship.getLocation().y)
    return command;
}


function getPriceByPortId(prices, portId) {
    return prices.filter(price => price.portId === portId)[0];
}

function getProductForLoad({goodsInPort, prices, ports, }) {
    const freeSpaceShip = ship.freeSpaceInShip( );
    const tradingPorts = ports.filter(port => !port.isHome);

    const products = tradingPorts.map((port, index) => {
        const price = getPriceByPortId(prices, port.portId);
        if (!price) return null;
        let optimalProduct = null;
        let max = 0;
        for (const product of goodsInPort) {
            if (price.hasOwnProperty(product.name)) {
                const amountInShip = Math.min(Math.floor(freeSpaceShip / product.volume), product.amount);
                const profit = price[product.name]*amountInShip;
                if (max < profit) {
                    optimalProduct = {
                        name: product.name,
                        amount: amountInShip
                    };
                    max = profit;
                }
            }
        }
        return {
            product: optimalProduct,
            priceInPort: price,
            port, index
        }
    });
    products.forEach(obj => {
        if (obj && obj.product && !lenToPorts.hasOwnProperty(obj.port.portId))
            lenToPorts[obj.port.portId] = Maths.distance(obj.port, homePort); // lazy init
    });
    const profitToPort = (obj) => obj && obj.product && productProfit(obj.priceInPort, obj.product, lenToPorts[obj.port.portId]);
    const profitObj = products.reduce((obj1, obj2, index) => {
        return (profitToPort(obj1) > profitToPort(obj2) ? obj1 : obj2);
    }, null);
    return profitObj && profitObj.product;
}

function productProfit(priceInPort, product, len) {
    return priceInPort[product.name]*product.amount / len;
}

function getProductForSale() {
    const priceWithAmount = (product) => product && [product.name]*product.amount;
    const product = ship.items.reduce((obj1, obj2) => {
        if (priceWithAmount(obj1) > priceWithAmount(obj2)) {
            return obj1;
        }
        return obj2;
    }, null);
    return product;
}


function profitOnSale(port, price) {
    if (!price) { return 0;}

    const profit = ship.items.map((val, i, arr) =>
            (price[val.name]*val.amount) / Maths.distance(ship, port)).reduce((a, b) => a+b, 0);

    return profit;
}



function findOptimalPort(ports) {
    //ports = portsCoordinates
    return ports.reduce((max_port, port) => {
        // console.log(port)
        // console.log(max_port)
        const profitFromCurrentPort = profitOnSale(port, port.prices);
        const profitFromMaxPort = profitOnSale(max_port, max_port.prices);
        if (profitFromCurrentPort > profitFromMaxPort) {
            return port;
        } else {
            return max_port;
        }
    }, ports[0]);
}




function goto(gameState) {
    const optimalPort = findOptimalPort(gameState.ports);
    if (optimalPort === undefined) return 'WAIT';

    let command;
    if (ship.y > optimalPort.y) {
        //if (isUnlockedWay(ship.x, ship.y-1)) {
            command = ship.moveToNorth()
        //}
    }
    if (ship.y < optimalPort.y) {
        //if (isUnlockedWay(ship.x, ship.y+1)) {
            command = ship.moveToSouth()
        //}
    }
    if (ship.x > optimalPort.x) {
        //if (isUnlockedWay(ship.x-1, ship.y)) {
            command = ship.moveToWest()
        //}
    }
    if (ship.x < optimalPort.x) {
        //if (isUnlockedWay(ship.x+1, ship.y)) {
            command = ship.moveToEast()
        //}
    }
    if (command === undefined) {
        command = ship.wait()
    }
    //console.log(optimalPort.x, optimalPort.y)
    return command;
}



class QElement {
    constructor(element, priority)
    {
        this.element = element;
        this.priority = priority;
    }
}

class PriorityQueue {
    constructor()
    {
        this.items = [];
    }

    enqueue(element, priority)    {
        // creating object from queue element
        var qElement = new QElement(element, priority);
        var contain = false;

        // iterating through the entire item array to add element at the correct location of the Queue
        for (var i = 0; i < this.items.length; i++) {
            if (this.items[i].priority > qElement.priority) {
                // Once the correct location is found it is enqueued
                this.items.splice(i, 0, qElement);
                contain = true;
                break;
            }
        }

        // if the element have the highest priority
        // it is added at the end of the queue
        if (!contain) {
            this.items.push(qElement);
        }
    }


    dequeue()    {
        // return the dequeued element and remove it.
        // if the queue is empty returns Underflow
        if (this.isEmpty())
            return "Underflow";
        return this.items.shift();
    }

    front()    {
        // returns the highest priority element in the Priority queue without removing it.
        if (this.isEmpty())
            return "No elements in Queue";
        return this.items[0];
    }

    rear()
    {
        // returns the lowest priorty element of the queue
        if (this.isEmpty())
            return "No elements in Queue";
        return this.items[this.items.length - 1];
    }
    isEmpty()
    {
        // return true if the queue is empty.
        return this.items.length === 0;
    }

    printPQueue()
    {
        var str = "";
        for (var i = 0; i < this.items.length; i++)
            str += this.items[i].element + " ";
        return str;
    }


}