window.appFunctions = {
    componentReady: function (componentName) {
        console.log(`${componentName} is ready`);
        window.appState = window.appState || {};
        window.appState[componentName] = { ready: true };
    }
};

function isLeafletReady() {
    return typeof InitializeLeafletMap === 'function' && typeof L !== 'undefined' && L.map;
}

function initializeLeafletMapWhenReady(hubs, incidents, routes, addNewHubOnclickEvent, dotNetHelper, hubToEdit) {
    if (isLeafletReady()) {
        InitializeLeafletMap(hubs, incidents, routes, addNewHubOnclickEvent, dotNetHelper, hubToEdit);
    } else {
        setTimeout(() => InitializeLeafletMap(hubs, incidents, routes, addNewHubOnclickEvent, dotNetHelper, hubToEdit), 100);
    }
}

function disposeLeafletMap() {
    if (window.map) {
        window.map.remove();
        window.map = null;
    }
    markers = [];
    hubs = [];
    scanners = [];
    markerClusterGroup = null;
    routesLayerGroup = null;
    newHubMarker = null;
    newHubArrow = null;
    newHubArrowHead = null;
    isDraggingNewHubArrowHead = null;
    dotNetHelper = null;
}

function GetTimeForDisplay(date) {
    var localTime = new Date(new Date(date).toLocaleString('en-US', {
        timeZone: 'America/New_York'
    }));

    var day = localTime.getDate();
    var month = localTime.getMonth() + 1;
    var year = localTime.getFullYear();
    var hours = localTime.getHours();
    var minutes = localTime.getMinutes();
    var seconds = localTime.getSeconds();

    return `${day}/${month}/${year}, ${hours}:${minutes}:${seconds}`;
}

function addEventHandlersToMobileVerticalMenu()
{
    var elements = document.getElementsByClassName('link-menu');

    //Collapse menu and submenus when opening a page from a link in the menu or a submenu
    for (var i = 0; i < elements.length; i++) {
        elements[i].addEventListener('click', function () {
            $('#menu-root').collapse('hide');
            $('#menu-queries-mobile').collapse('hide');
            $('#menu-configuration-mobile').collapse('hide');
        });
    }
}