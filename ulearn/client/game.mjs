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
    queue.push({...objSource, way: []}, 0);
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

    while (queue.length !== 0) {
        const node = queue.shift();
        function isEqualPosition(obj1, obj2) {
            return obj1.x === obj2.x && obj1.y === obj2.y;
        }
        if (isEqualPosition(node, objDestination)) {
            // console.log(visited);
            return node.way;
        }
        function manhattanDistance(obj1, obj2) {
            return Math.abs(obj1.x-obj2.x)+Math.abs(obj1.y-obj2.y);
        }

        visited[node.y][node.x] = true;
        for (const direction of directions) {
            const new_node = {
                x: node.x + direction.x,
                y: node.y + direction.y
            };
            if (isCorrectWay(new_node) && !visited[new_node.y][new_node.x]) {
                const {x, y} = new_node;
                new_node.way = [...node.way, {x, y}];
                queue.push(new_node, new_node.way.length + manhattanDistance(new_node, objDestination));
            }
        }
    }
    return [];
}
//////////////////////////

function getProductForLoad(goodsInPort) {
    const freeSpaceShip = ship.getFreeSpaceInShip();

    const products = tradePorts.map((port, index) => {
        const price = port.prices;
        if (!price) return null;
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

    for (const product of products) {
        if (product && product.product && !lenToPorts.hasOwnProperty(product.port.portId)) {
            lenToPorts[product.port.portId] = Maths.distance(product.port, homePort);
        }
    }

    const profitObj = maxElement(products, profitToPort)
    return profitObj && profitObj.product;
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
    const product = maxElement(ship.items, priceWithAmount);

    return product;
}


function profitOnSale(port) {
    if (port instanceof HomePort) return 0;
    if (!port.prices) return 0;

    const profit = ship.items.map((val, i, arr) =>
        (port.prices[val.name] * val.amount) / Maths.distance(ship, port))

    const maxProfit = profit.reduce((a, b) => a + b, 0);

    return maxProfit;
}


function findOptimalPort() {
    const portes = tradePorts;
    portes.push(homePort)
    //return maxElement(portes, profitOnSale, homePort)
    return portes.reduce((max_port, port) => {
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


class QElement {
    constructor(element, priority) {
        this.element = element;
        this.priority = priority;
    }
}

class PriorityQueue {
    constructor() {
        this._objs = [];
        this._length = 0;
    }

    heapUp(index) {
        const obj = this._objs[index];

        while (index > 0) {

            const parentIndex = Math.floor((index - 1) / 2);
            if (this._objs[parentIndex].priority <= obj.priority) {
                break;
            }

            this._objs[index] = this._objs[parentIndex];

            index = parentIndex;
        }


        this._objs[index] = obj;
    }

    heapDown(index) {
        const obj = this._objs[index];

        while (index < this._length) {
            const left = (index * 2) + 1;
            if (left >= this._length) {
                break;
            }

            let childObj = this._objs[left];
            let childIndex = left;

            const right = left + 1;
            if (right < this._length) {
                const rightObj = this._objs[right];
                if (rightObj.priority < childObj.priority) {
                    childObj = rightObj;
                    childIndex = right;
                }
            }

            if (childObj.priority >= obj.priority) {
                break;
            }

            this._objs[index] = childObj;

            index = childIndex;
        }

        this._objs[index] = obj;
    }

    push(key, priority) {
        this._objs.push({key, priority});
        this.heapUp(this._length);
        this._length++;
    }

    shift() {
        if (this._length === 0) {
            return undefined;
        }
        const obj = this._objs[0];

        this._length--;

        if (this._length > 0) {
            this._objs[0] = this._objs[this._length];
            this._objs.pop();
            this.heapDown(0);
        } else {
            this._objs.pop();
        }

        return obj.key;
    }

    get length() {
        return this._length;
    }
}