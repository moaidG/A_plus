function __print(msg) {
    var out = document.getElementById('output');
    if (out) out.style.display = 'block';
    var div = document.createElement('div');
    div.textContent = String(msg);
    out.appendChild(div);
}

let __xaml_0 = createCard("A+ Language", "icons.png");
let card1 = __xaml_0;
let __xaml_1 = createCard("Component System", "loge_m_ait.png");
let card2 = __xaml_1;
let B = __xaml_2;
let myBtn = __xaml_2;

let __root;
let __xaml_2;
let Panel;

function __init() {
    __root = document.getElementById('__root');
    __xaml_2 = document.getElementById('__xaml_2');
    Panel = document.getElementById('Panel');
}

function __xaml_ev_1() {
    __print((("Button '" + label) + "' clicked!"));
}

function __xaml_ev_2() {
    __print("Button clicked 2!");
}

function createCard(title, imgUrl) {
    let __xaml_3 = document.createElement('div');
    __xaml_3.id = '__xaml_3';
    __xaml_3.style.backgroundImage = 'url(' + imgUrl + ')';
    __xaml_3.style.width = "300";
    __xaml_3.style.height = "200";
    let __xaml_4 = document.createElement('span');
    __xaml_4.id = '__xaml_4';
    __xaml_4.textContent = title;
    __xaml_4.style.fontSize = "18";
    __xaml_4.style.color = "White";
    __xaml_4.style.textAlign = "Center";
    __xaml_3.appendChild(__xaml_4);
    return __xaml_3;
}

function createButton(label) {
    let __xaml_5 = document.createElement('button');
    __xaml_5.id = '__xaml_5';
    __xaml_5.textContent = label;
    __xaml_5.style.width = "150";
    __xaml_5.style.height = "40";
    __xaml_5.onclick = __xaml_ev_1;
    return __xaml_5;
}

function __xaml_ev_0() {
    __print("Button clicked 1!");
}

// Initialize
document.addEventListener('DOMContentLoaded', function() {
    __init();
    // Top-level logic
    __root.appendChild(createCard("A+ Language", "icons.png"));
    __root.appendChild(createCard("Component System", "loge_m_ait.png"));
    Panel.appendChild(__xaml_0);
    Panel.appendChild(__xaml_1);
    Panel.appendChild(__xaml_2);
    // Print app name
    __print('=== component_demo ===');
});
