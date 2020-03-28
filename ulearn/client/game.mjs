const SHIP_HOLD_SIZE = 368;

class Ship {
    x = 0;
    y = 0;
    items;

    constructor(gameState) {
        this.refreshShipState(gameState)
    }

    getLocation() {
        const loc = {x: this.x, y: this.y}
        return loc;
    }

    refreshShipState(gameState) {
        this.x = gameState.x;
        this.y = gameState.y;
        this.items = gameState.goods;
    }

    isInTradePort() {
        const portsArray = tradePorts.filter(port => this.weAreIn(port));
        return !!portsArray;
    }

    isInHomePort() {
        return this.weAreIn(homePort);
    }

    weAreIn(something) {
        return this.x === something.x && this.y === something.y;
    }

    notHaveItems() {
        return ship.items.length > 0
    }

    canLoadProduct(gameState) {
        return this.getFreeSpaceInShip() > 35 && !ship.notHaveItems() && ship.isHomePort(gameState.ship);
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

    needSale() {
        return ship.isInTradePort() && ship.weAreIn(findOptimalPort())
    }

    getFreeSpaceInShip() {
        return ship.items.reduce((acc, cur) => acc - productDesc[cur.name] * cur.amount, SHIP_HOLD_SIZE);
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



class Map {
    symbolMap;
    lastPiratesLocatation ;

    refreshPirates(pirates) {
        this.lastPiratesLocatation = matrixArray(this.Height, this.Width)
        const directions = [
            {x: -1, y:  0},
            {x:  1, y:  0},
            {x:  0, y: -1},
            {x:  0, y:  1},
        ];
        for(const pirate of pirates) {
            for (const direction of directions) {
                const x = pirate.x + direction.x;
                const y = pirate.y+direction.y;
                this.lastPiratesLocatation[y][x] = true;
            }
        }
        //console.log(this.rememberMapObjects)

        this.rememberMapObjects = [];
    }

    constructor(levelMap) {
        const matrix = levelMap.split('\n');
        for (let x = 0; x < matrix.length; x++) {
            matrix[x] = matrix[x].split("")
        }

        const width = matrix.length;
        const height = matrix[0].length
        let matrixAdjasment = matrixArray(width, height);
        this.lastPiratesLocatation = matrixArray(width, height)

        //console.log(matrixAdjasment)
        for (let x = 1; x < matrix.length - 1; x++) {
            for (let y = 1; y < matrix[x].length - 1; y++) {
                const currentCell = matrix[x][y];
                const neighbours = [
                    createMapObject(currentCell, x - 1, y),
                    createMapObject(currentCell,x + 1, y),
                    createMapObject(currentCell, x, y - 1),
                    createMapObject(currentCell, x, y + 1)
                ];
                if (currentCell !== "#") {
                    let childrens = [];
                    for (const neighbour of neighbours) {
                        if (matrix[neighbour.x][neighbour.y] !== "#") {
                            const innerMapObject = createMapObject(matrix[neighbour.x][neighbour.y], neighbour.x, neighbour.y)
                            childrens.push(innerMapObject)
                        }
                    }
                    const mapObject = createMapObject(currentCell, x, y, childrens)
                    matrixAdjasment[x][y] = mapObject;
                }
            }
        }
        this.symbolMap = matrixAdjasment;
    }


    get Height() {
        return this.symbolMap.length;
    }

    get Width() {
        return this.symbolMap[0].length;
    }

    Get(y, x) {
        if (this.lastPiratesLocatation[y][x]) return 0
        return this.symbolMap[y][x];
    }

    Set(y,x,value) {
        //console.log(this.symbolMap[y][x])
        this.symbolMap[y][x] = value;
    }

}

class QElement {
    constructor(element, priority) {
        this.element = element;
        this.priority = priority;
    }
}

class PriorityQueue {
    constructor() {
        this.items = [];
    }

    enqueue(element, priority) {
        // creating object from queue element
        const qElement = new QElement(element, priority);
        let contain = false;
        // iterating through the entire item array to add element at the correct location of the Queue
        for (let i = 0; i < this.items.length; i++) {
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

    dequeue() {
        // return the dequeued element and remove it.
        // if the queue is empty returns Underflow
        // if (this.isEmpty())
        //     return false
        return this.items.shift();
    }


    isEmpty() {
        // return true if the queue is empty.
        return this.items.length === 0;
    }

}

function matrixArray(rows, columns) {
    var arr = [];
    for (var i = 0; i < rows; i++) {
        arr[i] = [];
        for (var j = 0; j < columns; j++) {
            arr[i][j] = 0;//вместо i+j+1 пишем любой наполнитель. В простейшем случае - null
        }
    }
    return arr;
}

function createMapObject(symbol, x, y, neighbours = []) {
    let mapObject;

    switch (symbol) {
        case "O": {
            mapObject = new MapObject(symbol, x, y, neighbours, false, true)
            break;
        }
        case "H": {
            mapObject = new MapObject(symbol, x, y,  neighbours,true)
            break;
        }
        case "~": {
            mapObject = new MapObject(symbol, x, y, neighbours)
            break;
        }
    }
    return mapObject;
}


class MapObject {
    x;
    y;
    isHomePort;
    isTradePort;
    symbol;
    neighbours = [];

    constructor(symbol, x, y, neighbours=[], isHomePort = false, isTradePort = false) {
        this.symbol = symbol;
        this.x = x;
        this.y = y;
        this.neighbours = neighbours;
        this.isHomePort = isHomePort;
        this.isTradePort = isTradePort;
    }
}


let map;
let ship;
let distanceToPorts;
let homePort;
let productDesc;
let prev_port;
let prev_way;
let tradePorts;
export function startGame(levelMap, gameState) {

    tradePorts = [];
    homePort = {};
    distanceToPorts = {};
    productDesc = {};
    ship = new Ship(gameState.ship);
    homePort = {}

    map = new Map(levelMap, gameState.pirates);

    for (const product of gameState.goodsInPort) {
        productDesc[product.name] = product.volume
    }
    prev_port = null;
    prev_way = [];

    for (let gameStatePort of gameState.ports) {
        const currentPortId = gameStatePort.portId;
        gameStatePort.prices = gameState.prices.filter(price => price.portId === currentPortId)[0]
    }

    const homePortArray = gameState.ports.filter(port => port.isHome)[0];
    const portsCoordinatesArray = gameState.ports.filter(port => !port.isHome);

    homePort = new HomePort(homePortArray.portId, homePortArray.x, homePortArray.y);
    portsCoordinatesArray.forEach(port =>
        tradePorts.push(new TradingPort(port.portId, port.x, port.y, port.prices)))

}


export function getNextCommand(gameState) {
    ship.refreshShipState(gameState.ship);
    map.refreshPirates(gameState.pirates, gameState.ship);

    let command = 'WAIT';
    if (ship.isInHomePort() && needLoadProduct(gameState)) {
        const product = getProductForLoad(gameState);
        if (product)
            command = `LOAD ${product.name} ${product.amount}`;
        else command = gotoPort(gameState);
    } else if (ship.isInTradePort() && needSale(gameState)) {
        const product = getProductForSale(gameState);
        if (product)
            command = `SELL ${product.name} ${product.amount}`;
        else command = gotoPort(gameState);
    } else if (gameState.ship.goods.length > 0 || haveGoodsInPort(gameState)) { // уже загрузили товар
        // перемещаемся к цели
        command = gotoPort(gameState);
    }
    //console.log(needLoadProduct(gameState))
    //console.log(command);
    return command;
}



function searchWay(objSource, objDestination) {
    const queue = new PriorityQueue();
    queue.enqueue({...objSource, way: []}, 0);
    const visited = new Array(map.Height);
    for (let i = 0; i < map.Height; i++) {
        visited[i] = (new Array(map.Width).fill(false));
    }
    const directions = [
        {x: -1, y:  0},
        {x:  1, y:  0},
        {x:  0, y: -1},
        {x:  0, y:  1},
    ];

    const isCorrectWay = obj => obj.x >= 0 && obj.x < map.Width && obj.y >= 0 && obj.y < map.Height && map.Get(obj.y, obj.x) !== '#';

    while (queue.length !== 0) {
        const node = queue.dequeue();

        if (isEqualPosition(node.element, objDestination)) {
            // console.log(visited);
            return node.element.way;
        }

        visited[node.element.y][node.element.x] = true;
        for (const direction of directions) {
            const new_node = {
                x: node.element.x + direction.x,
                y: node.element.y + direction.y
            };
            if (isCorrectWay(new_node) && !visited[new_node.y][new_node.x]) {
                const {x, y} = new_node;
                new_node.way = [...node.element.way, {x, y}];
                queue.enqueue(new_node, new_node.way.length + manhattanDistance(new_node, objDestination));
            }
        }
    }
    return [];
}


function manhattanDistance(obj1, obj2) {
    return Math.abs(obj1.x-obj2.x)+Math.abs(obj1.y-obj2.y);
}


function distance(obj1, obj2) {
    if (isEqualPosition(obj1, obj2)) return 1;
    const wayLength = searchWay(obj1, obj2).length;
    return wayLength || Infinity;
}


function haveGoodsInPort(gameState) {
    return gameState.goodsInPort.length !== 0;
}





function needLoadProduct(gameState) {
    const freeSpace = ship.getFreeSpaceInShip();

    if (freeSpace >= 35) {
        const port = findOptimalPort(gameState);
        const price = port.prices;
        return port.isHome || gameState.goodsInPort.reduce(
            (acc, good) => acc || (price.hasOwnProperty(good.name) && good.volume < freeSpace),
            false);
    } else {
        return false;
    }
}


function getCurrentPort({ship, ports}) {
    const prts = ports.filter(port => isEqualPosition(port, ship));
    return prts.length === 1 ? prts[0] : null;
}



function onTradingPort(gameState) {
    const port = getCurrentPort(gameState);
    return port ? !port.isHome : false;
}


function onHomePort(gameState) {
    const port = getCurrentPort(gameState);
    return port ? port.isHome : false;
}

function isEqualPosition(obj1, obj2) {
    return obj1.x === obj2.x && obj1.y === obj2.y;
}

/**
 * считаем что корабль пуст
 */
function getProductForLoad({goodsInPort, prices, ports}) {
    const freeSpaceShip = ship.getFreeSpaceInShip();
    const tradingPorts = ports.filter(port => !port.isHome);
    const products = tradingPorts.map((port, index) => {
        const price = port.prices;
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
        if (obj && obj.product && !distanceToPorts.hasOwnProperty(obj.port.portId))
            distanceToPorts[obj.port.portId] = distance(obj.port, homePort); // lazy init
    });
    const profitToPort = (obj) => obj && obj.product && productProfit(obj.priceInPort, obj.product, distanceToPorts[obj.port.portId]);
    const profitObj = products.reduce((obj1, obj2, index) => {
        return (profitToPort(obj1) > profitToPort(obj2) ? obj1 : obj2);
    }, null);
    return profitObj && profitObj.product;
}


function needSale(gameState) {
    return gameState.ship.goods.length > 0 &&
        isEqualPosition(findOptimalPort(gameState), gameState.ship);
}


function getProductForSale({ship, prices, ports}) {
    const port = getCurrentPort({ship, ports});
    const priceOnCurrentPort = port.prices;
    const priceWithAmount = (product) => product && (priceOnCurrentPort[product.name]*product.amount);
    return ship.goods.reduce((obj1, obj2) => {
        return (priceWithAmount(obj1) > priceWithAmount(obj2) ? obj1 : obj2);
    }, null);
}


function productProfit(priceInPort, product, len) {
    return priceInPort[product.name]*product.amount / len;
}


function profitOnSale(ship, port, price) {
    let profit = 0;
    if (!port.isHome && price) {
        // оперирую расстоянием, считая выгоду как прибыль в еденицу растояни (так как и во времени)
        profit = ship.goods.map((val, i, arr) => {
            if (price.hasOwnProperty(val.name)) {
                return productProfit(price, val, manhattanDistance(ship, port));
            }
            return 0;
        }).reduce((a, b) => a+b, 0);
    }

    return profit;
}


function findOptimalPort({ship, ports, prices}) {
    let profitFromMaxPort = profitOnSale(ship, ports[0], ports[0].prices);
    let indexMax = 0;
    for (let i = 1; i < ports.length; i++) {
        const port = ports[i];
        const profit = profitOnSale(ship, port, port.prices);

        if (profit > profitFromMaxPort) {
            indexMax = i;
            profitFromMaxPort = profit;
        }
    }
    return ports[indexMax];
}

// Движение корабля
function gotoPort(gameState) {
    const ship = gameState.ship;
    const port = findOptimalPort(gameState);
    if (port === undefined) return 'WAIT';
    const way = searchWay(ship, port);
    const point = way[0] || port;

    if (prev_port && isEqualPosition(prev_port, port) && prev_way.length+1 === way.length)
        return 'WAIT';

    prev_port = port;
    prev_way = way;

    if (ship.y > point.y) {
        return 'N'; // — North, корабль движется вверх по карте
    }
    if (ship.y < point.y) {
        return 'S'; // — South, корабль движется вниз по карте
    }
    if (ship.x > point.x) {
        return 'W'; // — West, корабль движется влево по карте
    }
    if (ship.x < point.x) {
        return 'E'; // — East, корабль движется вправо по карте
    }
    return 'WAIT'
}