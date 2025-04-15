//Global variables
//#region

var scannerIconRed = L.icon({
    iconUrl: '/images/scanner-icon-red.png',
    iconSize: [30, 23],
    iconAnchor: [15, 23],
    popupAnchor: [0, -23],
    shadowSize: [41, 41]
});

var scannerBlinkingIconRed = L.icon({
    iconUrl: '/images/scanner-icon-red.png',
    iconSize: [30, 23],
    iconAnchor: [15, 23],
    popupAnchor: [0, -23],
    shadowSize: [41, 41],
    className: 'blinking-icon'
});

var scannerIconGreen = L.icon({
    iconUrl: '/images/scanner-icon-green.png',
    iconSize: [30, 23],
    iconAnchor: [15, 23],
    popupAnchor: [0, -23],
    shadowSize: [41, 41]
});

var scannerBlinkingIconGreen = L.icon({
    iconUrl: '/images/scanner-icon-green.png',
    iconSize: [30, 23],
    iconAnchor: [15, 23],
    popupAnchor: [0, -23],
    shadowSize: [41, 41],
    className: 'blinking-icon'
});

var scannerIconDisconnected = L.icon({
    iconUrl: '/images/scanner-icon-disconnected.png',
    iconSize: [30, 23],
    iconAnchor: [15, 23],
    popupAnchor: [0, -23],
    shadowSize: [41, 41]
});

var markers = [];
var hubs = [];
var scanners = [];

var map, markerClusterGroup, routesLayerGroup, newHubMarker, newHubArrow, newHubArrowHead, isDraggingNewHubArrowHead, dotNetHelper;

//#endregion

function InitializeLeafletMap(hubsJson, incidents, routes, addNewHubOnclickEvent, _dotNetHelper, hubToEdit) {
    if (map === null || map === undefined) {

        hubs = hubsJson;
        scanners = [];
        markers = [];
        dotNetHelper = _dotNetHelper;

        markerClusterGroup = L.markerClusterGroup({
            iconCreateFunction: function (cluster) {
                var markers = cluster.getAllChildMarkers();
                var childCount = cluster.getChildCount();

                var anyIncidents = markers.some(marker => marker.options.hasIncident);
                var iconUrl = anyIncidents ? '/images/scanner-icon-red.png' : '/images/scanner-icon-green.png';
                var iconClass = anyIncidents && markers.some(marker => marker.options.hasIncidentBlinkingIcon) ? 'blinking-icon' : "";

                return L.divIcon({
                    html: `<div style="display:flex;flex-direction:column;align-items:center;"><img class="${iconClass}" src="${iconUrl}" alt="icono scanner"><div style="width:25px;height:25px;margin:0;" class="marker-cluster-small"><span style="line-height:28px;">${childCount}</span></div></div>`,
                    className: 'marker-cluster',
                    iconSize: L.point(40, 40),
                    iconAnchor: L.point(20, 20),
                    popupAnchor: L.point(0, -20)
                });
            }
        });

        map = L.map('map').setView([40.7128, -74.0060], 10);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; OpenStreetMap contributors'
        }).addTo(map);

        for (const hub of hubs) {

            if (hubToEdit) {
                if (hubToEdit.latitude == hub.latitude && hubToEdit.longitude == hub.longitude) {
                    const latlng = { lat: hubToEdit.latitude, lng: hubToEdit.longitude };
                    addNewHubMarkerAndArrow(latlng);
                    continue;
                }
            }


            let marker = L.marker([hub.latitude, hub.longitude], { icon: scannerIconGreen, hasIncident: false });

            markers.push(marker);
            markerClusterGroup.addLayer(marker);

            hub.scanners.forEach(scanner => {
                scanners.push(scanner);
            });

            let popupContent = initializePopUpContent(hub);

            let incidentsByHub = incidents.filter(incident => incident.hubCode === hub.code);

            if (incidentsByHub.length > 0) {

                popupContent = addIncidentsToPopUpContent(incidentsByHub, popupContent);
                marker.setIcon(scannerBlinkingIconRed);
                marker.options.hasIncident = true;
                marker.options.hasIncidentBlinkingIcon = true;

            } else {
                marker.setIcon(scannerIconGreen);
                marker.options.hasIncident = false;
                marker.options.hasIncidentBlinkingIcon = false;
            }

            marker.bindPopup(popupContent);
            marker.on('click', function (e) {

                if (marker.options.hasIncidentBlinkingIcon) {
                    marker.setIcon(scannerIconRed);
                    marker.options.hasIncidentBlinkingIcon = false;
                    markerClusterGroup.refreshClusters();
                }
            });
        };

        routesLayerGroup = L.layerGroup().addTo(map);
        addRoutes(routes);

        map.addLayer(markerClusterGroup);
        markerClusterGroup.refreshClusters();

        if (addNewHubOnclickEvent) {
            map.on('click', function (e) {
                if (!isDraggingNewHubArrowHead) {
                    if (!newHubMarker) {
                        addNewHubMarkerAndArrow(e.latlng);
                    } else {
                        newHubMarker.setLatLng(e.latlng);
                        newHubArrow.setLatLngs([newHubMarker.getLatLng(), newHubMarker.getLatLng()]);
                        newHubArrowHead.setLatLng(newHubMarker.getLatLng(), newHubMarker.getLatLng());
                    }
                    dotNetHelper.invokeMethodAsync('UpdateFormCoordinates', e.latlng.lat, e.latlng.lng);
                }
            });
        }

        mapIsInitialized = true;
    }
}

function RefreshLeafletMap(newHubsJson, newIncidents, newRoutes, addNewHubOnclickEvent, _dotNetHelper, hubToEdit) {
    if (map !== null && map !== undefined) {
        // Clear existing markers, routes, and layers
        markerClusterGroup.clearLayers();
        routesLayerGroup.clearLayers();
        markers = [];
        scanners = [];

        // Update data variables
        hubs = newHubsJson;
        dotNetHelper = _dotNetHelper;

        // Add new hubs and markers
        for (const hub of hubs) {

            if (hubToEdit) {
                if (hubToEdit.latitude == hub.latitude && hubToEdit.longitude == hub.longitude) {
                    const latlng = { lat: hubToEdit.latitude, lng: hubToEdit.longitude };
                    addNewHubMarkerAndArrow(latlng);
                    continue;
                }
            }

            let marker = L.marker([hub.latitude, hub.longitude], { icon: scannerIconGreen, hasIncident: false });

            markers.push(marker);
            markerClusterGroup.addLayer(marker);

            hub.scanners.forEach(scanner => {
                scanners.push(scanner);
            });

            let popupContent = initializePopUpContent(hub);

            let incidentsByHub = newIncidents.filter(incident => incident.hubCode === hub.code);

            if (incidentsByHub.length > 0) {
                popupContent = addIncidentsToPopUpContent(incidentsByHub, popupContent);
                marker.setIcon(scannerBlinkingIconRed);
                marker.options.hasIncident = true;
                marker.options.hasIncidentBlinkingIcon = true;
            } else {
                marker.setIcon(scannerIconGreen);
                marker.options.hasIncident = false;
                marker.options.hasIncidentBlinkingIcon = false;
            }

            marker.bindPopup(popupContent);
            marker.on('click', function (e) {
                if (marker.options.hasIncidentBlinkingIcon) {
                    marker.setIcon(scannerIconRed);
                    marker.options.hasIncidentBlinkingIcon = false;
                    markerClusterGroup.refreshClusters();
                }
            });
        }

        // Add new routes
        addRoutes(newRoutes);

        // Refresh the map's clustering layer
        markerClusterGroup.refreshClusters();

        // Handle adding new hub on click event
        if (addNewHubOnclickEvent) {
            map.off('click'); // Remove existing click event if any
            map.on('click', function (e) {
                if (!isDraggingNewHubArrowHead) {
                    if (!newHubMarker) {
                        addNewHubMarkerAndArrow(e.latlng);
                    } else {
                        newHubMarker.setLatLng(e.latlng);
                        newHubArrow.setLatLngs([newHubMarker.getLatLng(), newHubMarker.getLatLng()]);
                        newHubArrowHead.setLatLng(newHubMarker.getLatLng(), newHubMarker.getLatLng());
                    }
                    dotNetHelper.invokeMethodAsync('UpdateFormCoordinates', e.latlng.lat, e.latlng.lng);
                }
            });
        }
    }
}

// Hubs and Scanners map handling
//#region

function initializePopUpContent(hub) {
    let popupContent = `Hub: <strong><a href="/monitoring?hub-code=${hub.code}" target="_blank">${hub.name}</a></strong> (<strong>${hub.code}</strong>)<br>Scanners:<br><strong>`;

    for (const scanner of hub.scanners) {
        const svgArrow = createSvgArrow(scanner.gradosDireccionLane);
        if (scanner.isConnected) {
            popupContent += '<a href="/monitoring?scanner-code=' + scanner.code + '" target="_blank">' + 'Lane ' + scanner.code.split("AC")[1] + '</a>' + svgArrow + '</strong> Address: <strong>' + scanner.laneDestination + '<br>';
        } else {
            popupContent += '<span style="color:red;">' + 'Scanner' + 'Lane ' + scanner.code.split("AC")[1] + svgArrow + '<span style="font-size:10px;"> (Disconnected)</span></span>' + '<br>';
        }
    }

    popupContent += `</strong>`;

    return popupContent;
}

function createSvgArrow(degrees, color = 'green', size = 30) {
    const halfSize = size / 2;
    const arrowLength = halfSize * 0.9; // Length of the arrow relative to the size
    const angleRad = degrees * (Math.PI / 180); // Convert degrees to radians

    // Calculate the endpoint of the arrow
    const arrowX = halfSize + arrowLength * Math.cos(angleRad);
    const arrowY = halfSize - arrowLength * Math.sin(angleRad);

    // Create SVG element
    const svg = `<svg width="${size}" height="${size}" viewBox="0 0 ${size} ${size}" xmlns="http://www.w3.org/2000/svg" style="vertical-align: middle; margin-left: 4px;">
                    <line x1="${halfSize}" y1="${halfSize}" x2="${arrowX}" y2="${arrowY}" stroke="${color}" stroke-width="2" />
                    <polygon points="${arrowX},${arrowY} ${arrowX - 6 * Math.cos(angleRad - Math.PI / 6)},${arrowY + 6 * Math.sin(angleRad - Math.PI / 6)} ${arrowX - 6 * Math.cos(angleRad + Math.PI / 6)},${arrowY + 6 * Math.sin(angleRad + Math.PI / 6)}" fill="${color}" />
                 </svg>`;

    return svg;
}

//function createSvgLaneWithArrow(degrees, color = 'green', size = 35, laneWidth = 12) {
//    const halfSize = size / 2;
//    const laneLength = halfSize * 0.9; // Length of the lane relative to the size
//    const arrowLength = laneLength * 0.8; // Length of the arrow relative to the lane length
//    const angleRad = degrees * (Math.PI / 180); // Convert degrees to radians

//    // Calculate the endpoints of the lane
//    const x1 = halfSize + laneLength * Math.cos(angleRad);
//    const y1 = halfSize - laneLength * Math.sin(angleRad); // SVG y-coordinates are from top to bottom
//    const offsetX = laneWidth * Math.sin(angleRad) / 2;
//    const offsetY = laneWidth * Math.cos(angleRad) / 2;

//    // Calculate the endpoints of the parallel lines
//    const line1StartX = halfSize - offsetX;
//    const line1StartY = halfSize + offsetY;
//    const line1EndX = x1 - offsetX;
//    const line1EndY = y1 + offsetY;

//    const line2StartX = halfSize + offsetX;
//    const line2StartY = halfSize - offsetY;
//    const line2EndX = x1 + offsetX;
//    const line2EndY = y1 - offsetY;

//    // Calculate the endpoint of the arrow
//    const arrowX = halfSize + arrowLength * Math.cos(angleRad);
//    const arrowY = halfSize - arrowLength * Math.sin(angleRad);

//    // Create SVG element
//    const svg = `<svg width="${size}" height="${size}" xmlns="http://www.w3.org/2000/svg" style="vertical-align: middle; margin-left: 4px;">
//                    <line x1="${line1StartX}" y1="${line1StartY}" x2="${line1EndX}" y2="${line1EndY}" stroke="${color}" stroke-width="2" />
//                    <line x1="${line2StartX}" y1="${line2StartY}" x2="${line2EndX}" y2="${line2EndY}" stroke="${color}" stroke-width="2" />
//                    <line x1="${halfSize}" y1="${halfSize}" x2="${arrowX}" y2="${arrowY}" stroke="${color}" stroke-width="2" />
//                    <polygon points="${arrowX},${arrowY} ${arrowX - 6 * Math.cos(angleRad - Math.PI / 6)},${arrowY + 6 * Math.sin(angleRad - Math.PI / 6)} ${arrowX - 6 * Math.cos(angleRad + Math.PI / 6)},${arrowY + 6 * Math.sin(angleRad + Math.PI / 6)}" fill="${color}" />
//                 </svg>`;

//    return svg;
//}


function ChangeScannerConnectionStateInLeafletMap(scannerCode, isConnected) {
    let scannerToUpdate = scanners.find(scanner => scanner.code === scannerCode);

    if (scannerToUpdate) {
        let markerIndex = markers.findIndex(marker =>
            marker.getLatLng().lat === scannerToUpdate.latitude &&
            marker.getLatLng().lng === scannerToUpdate.longitude);

        if (markerIndex !== -1) {
            let marker = markers[markerIndex];

            //let newIcon = isConnected ? scannerIconGreen : scannerIconDisconnected;
            //marker.setIcon(newIcon);
            //marker.options.hasScannerDisconnectedIcon = !isConnected;
            //markerClusterGroup.refreshClusters();

            let popupContent = marker.getPopup().getContent();

            if (!isConnected) {
                popupContent = popupContent.replace(scannerCode, "<span style='color:red;'>" + scannerCode + "</span>" + " <span style='font-size:8px;'>(Disconnected)</span>");
            } else {
                popupContent = popupContent.replace("<span style='color:red;'>" + scannerCode + "</span>" + " <span style='font-size:8px;'>(Disconnected)</span>", scannerCode);
            }

            marker.bindPopup(popupContent);
        }

        //marker.openPopup();

    }
    else
    {
        console.log(`Scanner with code ${scannerCode} not found.`);
    }
}

// #endregion

// Incidents handling
//#region
function RefreshIncidentsInLeafletMap(incidents) {

    hubs.forEach(hub => {

        let markerIndex = markers.findIndex(marker =>
            marker.getLatLng().lat === hub.latitude &&
            marker.getLatLng().lng === hub.longitude);

        if (markerIndex !== -1) {
            let marker = markers[markerIndex];
            let popupContent = initializePopUpContent(hub);

            let incidentsByHub = incidents.filter(incident => incident.hubCode === hub.code);

            if (incidentsByHub.length > 0) {

                popupContent = addIncidentsToPopUpContent(incidentsByHub, popupContent);
                marker.setIcon(scannerBlinkingIconRed);
                marker.options.hasIncident = true;
                marker.options.hasIncidentBlinkingIcon = true;
                markerClusterGroup.refreshClusters();
            } else {
                marker.setIcon(scannerIconGreen);
                marker.options.hasIncident = false;
                marker.options.hasIncidentBlinkingIcon = false;
                markerClusterGroup.refreshClusters();
            }
            marker.bindPopup(popupContent);
        }
    });
}

function AddIncidentToLeafletMap(code, assetId, unexpectedStop, idleTooLong, earlyDeparture, readDate, hubCode, scannerCode) {
    let scannerToUpdate = scanners.find(scanner => scanner.code === scannerCode);

    if (scannerToUpdate) {
        let markerIndex = markers.findIndex(marker =>
            marker.getLatLng().lat === scannerToUpdate.latitude &&
            marker.getLatLng().lng === scannerToUpdate.longitude);

        if (markerIndex !== -1) {
            let marker = markers[markerIndex];

            let newIcon = !unexpectedStop && !idleTooLong && !earlyDeparture ? scannerIconGreen : scannerBlinkingIconRed;
            marker.setIcon(newIcon);
            marker.options.hasIncident = unexpectedStop || idleTooLong || earlyDeparture;
            marker.options.hasIncidentBlinkingIcon = unexpectedStop || idleTooLong || earlyDeparture;
            markerClusterGroup.refreshClusters();

            let popupContent = marker.getPopup().getContent();

            if (!popupContent.includes("Incidents")) {
                popupContent += `<p style="color:red; font-weight:bold; margin-bottom:10px"><i class="ti-bell"></i> Incidents <i class="ti-trash" onclick="clearIncidentsInPopUpContent('${hubCode}')" style="float:right; color:grey; cursor:pointer;"></i></p>`;
                popupContent += `<div id="incidents-${hubCode}" style="max-height:250px; overflow-y:auto;">`;

                popupContent += `<span style="color:green"><i class="ti-signal"></i> (Lane ${scannerCode.split("AC")[1]})</span> <span style="font-size:8pt; font-style:italic;"> ${GetTimeForDisplay(readDate)}</span></br>`;

                if (unexpectedStop) {
                    popupContent += `Asset <a href='assets/${assetId}' target="_blank">${code}</a> has stopped in the wrong hub</br>`;
                }
                if (idleTooLong) {
                    popupContent += `Asset <a href='assets/${assetId}' target="_blank">${code}</a> has stopped for too long in this hub</br>`;
                }
                if (earlyDeparture) {
                    popupContent += `Asset <a href='assets/${assetId}' target="_blank">${code}</a> has departed too early from this hub</br>`;
                }           
            } else {
                let parser = new DOMParser();
                let popupContentHtml = parser.parseFromString(popupContent, "text/html");

                let incidentsContainer = popupContentHtml.getElementById(`incidents-${hubCode}`);
                if (incidentsContainer) {
                    let newIncidentMarkup = `<span style="color:green"><i class="ti-signal"></i> (Lane ${scannerCode.split("AC")[1]})</span> <span style="font-size:8pt; font-style:italic;"> ${GetTimeForDisplay(readDate)}</span><br/>`;

                    if (unexpectedStop) {
                        popupContent += `Asset <a href='assets/${assetId}' target="_blank">${code}</a> has stopped in the wrong hub</br>`;
                    }
                    if (idleTooLong) {
                        popupContent += `Asset <a href='assets/${assetId}' target="_blank">${code}</a> has stopped for too long in this hub</br>`;
                    }
                    if (earlyDeparture) {
                        popupContent += `Asset <a href='assets/${assetId}' target="_blank">${code}</a> has departed too early from this hub</br>`;
                    }

                    newIncidentMarkup += `<div style="height:5px;"></div>`;

                    incidentsContainer.insertAdjacentHTML('afterbegin', newIncidentMarkup);
                }

                let serializer = new XMLSerializer();
                popupContent = serializer.serializeToString(popupContentHtml.documentElement);
            }

            marker.bindPopup(popupContent);
        }

        //marker.openPopup();
    }
    else {
        console.log(`Scanner with code ${scannerCode} not found.`);
    }
}

function addIncidentsToPopUpContent(incidentsByHub, popupContent) {
    if (incidentsByHub.length > 0) {
        popupContent += `<p style="color:red; font-weight:bold; margin-bottom:10px"><i class="ti-bell"></i> Incidents <i class="ti-trash" onclick="clearIncidentsInPopUpContent('${incidentsByHub[0].hubCode}')" style="float:right; color:grey; cursor:pointer;"></i></p>`;
        popupContent += `<div id="incidents-${incidentsByHub[0].hubCode}" style="max-height:250px; overflow-y:auto;">`;

        incidentsByHub.forEach((incident, index) => {
            popupContent += `<span style="color:green;margin-top:5px;"><i class="ti-signal"></i> (Lane ${incident.scannerCode.split("AC")[1]})</span> `;
            popupContent += `<span style="font-size:8pt; font-style:italic;"> ${GetTimeForDisplay(incident.dateTime)}</span></br>`; 

            if (incident.incidentType == 1) {
                popupContent += `Asset <a href='assets/${incident.assetId}' target="_blank">${incident.assetCode}</a> has stopped in the wrong hub</br>`;
            }
            if (incident.incidentType == 2) {
                popupContent += `Asset <a href='assets/${incident.assetId}' target="_blank">${incident.assetCode}</a> has stopped for too long in this hub</br>`;
            }
            if (incident.incidentType == 3) {
                popupContent += `Asset <a href='assets/${incident.assetId}' target="_blank">${incident.assetCode}</a> has departed too early from this hub</br>`;
            }      
            if (index !== incidentsByHub.length - 1) {
                popupContent += `<div style="height:5px;"></div>`;
            }
        });
        popupContent += `</div>`;
    }
    return popupContent;
}

function clearIncidentsInPopUpContent(hubCode) {
    let hub = hubs.find(hub => hub.code === hubCode);
    let markerIndex = markers.findIndex(marker =>
        marker.getLatLng().lat === hub.latitude &&
        marker.getLatLng().lng === hub.longitude);

    if (markerIndex !== -1) {
        let marker = markers[markerIndex];
        marker.setIcon(scannerIconGreen);
        marker.options.hasIncident = false;
        marker.options.hasIncidentBlinkingIcon = false;
        markerClusterGroup.refreshClusters();

        let popupContent = initializePopUpContent(hub);
        marker.bindPopup(popupContent);
    }
}

//#endregion


//Routes handling
//#region
function addRoutes(routes) {
    routes.forEach(route => {
        if (route.geoJson) {
            var geoJsonData = JSON.parse(route.geoJson);
            L.geoJSON(geoJsonData, {
                style: function (feature) {
                    if (feature.geometry.type === 'LineString' || feature.geometry.type === 'Polygon') {
                        return {
                            color: route.color,
                            weight: 4,
                            opacity: 0.8
                        };
                    }
                },
                filter: function (feature, layer) {
                    return feature.geometry.type !== 'Point';
                },
                onEachFeature: function (feature, layer) {
                    //if (feature.properties && feature.properties.Name) {
                    //    layer.bindPopup(feature.properties.Name);
                    //}
                    layer.bindPopup(route.name ?? feature.properties.Name);
                }
            }).addTo(routesLayerGroup);
        }
    });
}

function clearRoutes() {
    routesLayerGroup.clearLayers();
}
function updateRoutes(newRoutes) {
    clearRoutes();
    addRoutes(newRoutes);
}
function hideRoutes() {
    map.removeLayer(routesLayerGroup);
}
function showRoutes() {
    map.addLayer(routesLayerGroup);
}

//#endregion


//Pages "/hubs/" functions
//#region

function addNewHubMarkerAndArrow(latlng) {
    //var newHubMarker = L.marker(latlng, { icon: scannerBlinkingIconGreen, draggable: true }).addTo(map);
    newHubMarker = L.circleMarker(latlng, { color: 'green', radius: 8, draggable: true }).addTo(map);
    const newHubArrowHeadLatLng = { lat: latlng.lat, lng: latlng.lng };

    const dotIcon = L.divIcon({
        className: 'custom-div-icon',
        html: `<div class="dot-icon" style="background-color: green; width: 10px; height: 10px; border-radius: 50%;"></div>`,
        iconSize: [10, 10],
        iconAnchor: [5, 5]
    });

    newHubArrowHead = L.marker(newHubArrowHeadLatLng, {
        draggable: true,
        icon: dotIcon
    }).addTo(map);

    newHubArrow = L.polyline([latlng, newHubArrowHeadLatLng], { color: 'green', weight: 2 }).addTo(map);

    newHubArrowHead.on('dragstart', function (e) {
        isDraggingNewHubArrowHead = true;
    });
    newHubArrowHead.on('drag', function (e) {
        newHubArrow.setLatLngs([newHubMarker.getLatLng(), newHubArrowHead.getLatLng()]);
        const angle = calculateAngle(newHubMarker.getLatLng(), newHubArrowHead.getLatLng());
        dotNetHelper.invokeMethodAsync('UpdateFormLaneDirectionDegrees', angle.toFixed(2));
    });
    newHubArrowHead.on('dragend', function (e) {
        setTimeout(function () {
            isDraggingNewHubArrowHead = false;
        }, 100);
    });
}

function updateMapFromNewHubCoordinates(lat, lng, routes) {
    const latlng = { lat: lat, lng: lng };
    if (!newHubMarker) {
        addNewHubMarkerAndArrow(latlng);
    } else {
        newHubMarker.setLatLng(latlng);
        newHubArrow.setLatLngs([newHubMarker.getLatLng(), newHubMarker.getLatLng()]);
        newHubArrowHead.setLatLng(newHubMarker.getLatLng(), newHubMarker.getLatLng());
    }

    updateRoutes(routes);

    //map.setView(latlng, 15);
}

function calculateAngle(start, end) {
    const dy = end.lat - start.lat;
    const dx = end.lng - start.lng;
    const theta = Math.atan2(dy, dx); // radians
    const angle = theta * (180 / Math.PI); // degrees
    return (angle + 360) % 360; // Normalize angle
}

//#endregion