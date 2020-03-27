const MAX_LOAD_SHIP = 368;

let tradePorts = [];
let homePort = {};
let ship;
let map = [[]];
let lenToPorts = {};
let productDesc = {};


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

    isHomePort() {
        return this.weAreIn(homePort);
    }

    weAreIn(something) {
        return this.x === something.x && this.y === something.y;
    }

    notHaveItems() {
        return ship.items.length > 0
    }

    canLoadProduct(gameState) {
        return !ship.notHaveItems() && ship.isHomePort(gameState.ship);
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
        return ship.notHaveItems() && ship.isInTradePort() && ship.weAreIn(findOptimalPort())
    }

    getFreeSpaceInShip() {
        return ship.items.reduce((acc, cur) => acc - productDesc[cur.name] * cur.amount, MAX_LOAD_SHIP);
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

    static productProfit(priceInPort, product, len) {
        return priceInPort[product.name] * product.amount / len;
    }

    static amountInShip(freeSpaceShip, product) {
        return  Math.min(Math.floor(freeSpaceShip / product.volume), product.amount);

    }
}


class MapObject {
    x;
    y;
    isHomePort;
    isTradePort;

    constructor(x, y, isHomePort = false, isTradePort = false) {
        this.x = x;
        this.y = y;
        this.isHomePort = isHomePort;
        this.isTradePort = isTradePort;
    }
}


class Node {
    childrens = [];
    mapObject;
    parent;

    constructor(mapObject, parent = null) {
        this.mapObject = mapObject;
        this.parent = parent;
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

function createMapObject(symbol, x, y) {
    let mapObject;
    switch (symbol) {
        case "O": {
            mapObject = new MapObject(x, y, false, true)
            break;
        }
        case "H": {
            mapObject = new MapObject(x, y, true)
            break;
        }
        case "~": {
            mapObject = new MapObject(x, y)
            break;
        }
    }
    return mapObject;
}




export function startGame(levelMap, gameState) {
    //console.log(levelMap)
    //parseMap(levelMap);
    tradePorts = [];
    homePort = {};
    map = [[]];
    lenToPorts = {};
    productDesc = {};
    ship = new Ship(gameState.ship);
    homePort = {}
    mapLevel = new GameMap(levelMap);
    //console.log(mapLevel)

    for (let gameStatePort of gameState.ports) {
        const currentPortId = gameStatePort.portId;
        gameStatePort.prices = gameState.prices.filter(price => price.portId === currentPortId)[0]
    }

    const homePortArray = gameState.ports.filter(port => port.isHome)[0];
    const portsCoordinatesArray = gameState.ports.filter(port => !port.isHome);

    homePort = new HomePort(homePortArray.portId, homePortArray.x, homePortArray.y);
    portsCoordinatesArray.forEach(port =>
        tradePorts.push(new TradingPort(port.portId, port.x, port.y, port.prices)))

    gameState.goodsInPort.forEach(good => {
        productDesc[good.name] = good.volume;
    });
}


export function getNextCommand(gameState) {
    let command = 'WAIT';
    ship.refreshShipState(gameState.ship);

    if (ship.canLoadProduct(gameState)) {
        const product = getProductForLoad(gameState.goodsInPort);
        if (product)
            command = `LOAD ${product.name} ${product.amount}`
    } else if (ship.needSale(gameState)) {
        const product = getProductForSale();
        command = `SELL ${product.name} ${product.amount}`
    } else {
        command = goto(gameState);
    }
    return command;
}


///////////////////
let mapLevel;
class GameMap {
    symbolMap;
    nodeMap;

    constructor(levelMap) {
        const matrix = levelMap.split('\n');
        for (let x = 0; x < matrix.length; x++) {
            matrix[x] = matrix[x].split("")
        }

        const width = matrix.length;
        const height = matrix[0].length
        let matrixAdjasment = matrixArray(width, height);
        //console.log(matrixAdjasment)
        for (let x = 1; x < matrix.length - 1; x++) {
            for (let y = 1; y < matrix[x].length - 1; y++) {
                const currentCell = matrix[x][y];
                const leftCell = new MapObject(x - 1, y)
                const rightCell = new MapObject(x + 1, y);
                const downCell = new MapObject(x, y - 1);
                const upCell = new MapObject(x, y + 1);
                const neighbours = [leftCell, rightCell, downCell, upCell];
                if (currentCell !== "#") {
                    const mapObject = createMapObject(currentCell, x, y)
                    let node = new Node(mapObject)
                    for (let i = 0; i < neighbours.length; i++) {
                        if (matrix[neighbours[i].x][neighbours[i].y] !== "#") {
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
        this.nodeMap = matrixAdjasment;
        this.symbolMap = matrix;
    }


    get Height() {
        return this.symbolMap.length;
    }

    get Width() {
        return this.symbolMap[0].length;
    }

    Get(y, x) {
        return this.symbolMap[y][x];
    }

}
function manivrateToPort(objSource, objDestination) {
    const queue = new PriorityQueue();
    queue.enqueue({...objSource, way: []}, 0);
    const visited = new Array(mapLevel.Height);
    for (let i = 0; i < mapLevel.Height; i++) {
        visited[i] = (new Array(mapLevel.Width).fill(false));
    }
    const directions = [
        {x: -1, y:  0},
        {x:  1, y:  0},
        {x:  0, y: -1},
        {x:  0, y:  1},
    ];

    const isCorrectWay = obj => obj.x >= 0 && obj.x < mapLevel.Width && obj.y >= 0 && obj.y < mapLevel.Height && mapLevel.Get(obj.y, obj.x) !== '#';

    while (!queue.isEmpty()) {
        const node = queue.dequeue();
        function isEqualPosition(obj1, obj2) {
            return obj1.x === obj2.x && obj1.y === obj2.y;
        }
        if (isEqualPosition(node.element, objDestination)) {
            return node.element.way;
        }
        function manhattanDistance(obj1, obj2) {
            return Math.abs(obj1.x-obj2.x)+Math.abs(obj1.y-obj2.y);
        }

        //console.log(node)
        visited[node.element.y][node.element.x] = true;
        //debugger
        for (const direction of directions) {
            const new_node = {
                x: node.element.x + direction.x,
                y: node.element.y + direction.y
            };
            //console.log(new_node)
            if (isCorrectWay(new_node) && !visited[new_node.y][new_node.x]) {
                const {x, y} = new_node;
                //console.log(new_node.way)
                new_node.way = [...node.element.way, {x, y}];
                queue.enqueue(new_node, new_node.way.length + manhattanDistance(new_node, objDestination));
                //console.log(queue.printPQueue())

            }
        }
        //console.log(node.element.y, node.element.x)
        //console.log(visited)
    }
    return [];
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

    dequeue() {
        // return the dequeued element and remove it.
        // if the queue is empty returns Underflow
        if (this.isEmpty())
            return false
        return this.items.shift();
    }

    front() {
        // returns the highest priority element in the Priority queue without removing it.
        if (this.isEmpty())
            return false
        return this.items[0];
    }

    rear() {
        // returns the lowest priorty element of the queue
        if (this.isEmpty())
            return false
        return this.items[this.items.length - 1];
    }

    isEmpty() {
        // return true if the queue is empty.
        return this.items.length === 0;
    }

    printPQueue() {
        var str = "";
        for (var i = 0; i < this.items.length; i++)
            str += this.items[i].element + " ";
        return str;
    }
}

//////////////////////////
function generateProducts(goodsInPort, freeSpaceShip) {
    const products = tradePorts.map((port, index) => {
        if (!port.prices) return null;
        const price = port.prices;
        let optimalProduct = null;
        let max = 0;
        for (const product of goodsInPort) {
            if (price.hasOwnProperty(product.name)) {
                const amountInShip = Maths.amountInShip(freeSpaceShip, product);
                const profit = price[product.name] * amountInShip;
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
    return products;
}


function getProductForLoad(goodsInPort) {
    const freeSpaceShip = ship.getFreeSpaceInShip();

    const products = generateProducts(goodsInPort, freeSpaceShip);

    for (const product of products) {
        if (product && product.product && !lenToPorts.hasOwnProperty(product.port.portId)) {
            lenToPorts[product.port.portId] = Maths.distance(product.port, homePort);
        }
    }

    const maxCostForProduct = maxElement(products, profitToPort)
    return maxCostForProduct && maxCostForProduct.product;
}


function profitToPort(obj) {
    return obj && obj.product && Maths.productProfit(obj.priceInPort, obj.product, lenToPorts[obj.port.portId]);
}

function maxElement(array, comparator, reduceDefaultValue=null) {
    const product = array.reduce((obj1, obj2) => {
        if (comparator(obj1) > comparator(obj2)) {
            return obj1;
        }
        return obj2;
    }, reduceDefaultValue);
    return product;
}


function getProductForSale() {
    const priceWithAmount = (product) => product && [product.name] * product.amount;
    return maxElement(ship.items, priceWithAmount);
}


function profitOnSale(port) {
    if (port instanceof HomePort || !port.prices) return 0;

    const profit = ship.items.map(function(val, i, arr) {
        return (port.prices[val.name] * val.amount) / Maths.distance(ship, port)
    })

    return profit.reduce((a, b) => a + b, 0);
}


function findOptimalPort() {
    const localPorts = tradePorts;
    localPorts.push(homePort)
    //return maxElement(portes, profitOnSale, homePort)
    return localPorts.reduce((max_port, port) => {
        if (profitOnSale(max_port) < profitOnSale(port)) {
            return port;
        } else {
            return max_port;
        }
    }, homePort);
}


function goto() {
    const optimalPort = findOptimalPort();
    if (optimalPort === undefined) return 'WAIT';
    const way = manivrateToPort(ship, optimalPort);
    console.log(way)
    const point = way[0] || optimalPort;

    let command;
    if (ship.y > point.y) {
        command = ship.moveToNorth()
    }
    if (ship.y < point.y) {
        command = ship.moveToSouth()
    }
    if (ship.x > point.x) {
        command = ship.moveToWest()
    }
    if (ship.x < point.x) {
        command = ship.moveToEast()
    }
    if (command === undefined) {
        command = ship.wait()
    }
    return command;
}


