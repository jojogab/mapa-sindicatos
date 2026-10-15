window.cnesMapa = (() => {
    const statesUrl = "https://raw.githubusercontent.com/codeforgermany/click_that_hood/main/public/data/brazil-states.geojson";
    const densityColors = ["#fee2e2", "#fca5a5", "#f87171", "#ef4444", "#991b1b"];
    const noDataColor = "#d4d4d8";

    let map;
    let dotNetReference;
    let statesLayer;
    let densityLegend;
    let countsByUf = {};
    let densityBreaks = [];
    let darkTheme = true;
    let selectedUf = "";

    function getUf(feature) {
        return (feature.properties.sigla ?? feature.properties.abbrev ?? "").toUpperCase();
    }

    function getCount(feature) {
        return countsByUf[getUf(feature)] ?? 0;
    }

    function calculateDensityBreaks() {
        const values = Object.values(countsByUf)
            .filter(count => count > 0)
            .sort((first, second) => first - second);

        densityBreaks = [];
        if (values.length === 0) {
            return;
        }

        for (let index = 1; index <= densityColors.length; index++) {
            const valueIndex = Math.ceil(index * values.length / densityColors.length) - 1;
            const cutoff = values[valueIndex];
            if (densityBreaks.at(-1) !== cutoff) {
                densityBreaks.push(cutoff);
            }
        }
    }

    function getDensityColor(count) {
        if (count <= 0) {
            return noDataColor;
        }

        const colorIndex = densityBreaks.findIndex(cutoff => count <= cutoff);
        return densityColors[colorIndex < 0 ? densityColors.length - 1 : colorIndex];
    }

    function stateStyle(feature) {
        const uf = getUf(feature);
        const selected = uf.toUpperCase() === selectedUf;
        return {
            color: selected ? (darkTheme ? "#ffffff" : "#18181b") : darkTheme ? "#52525b" : "#ffffff",
            fillColor: getDensityColor(getCount(feature)),
            fillOpacity: darkTheme ? 0.88 : 0.9,
            weight: selected ? 3 : 1.2
        };
    }

    function refreshStateStyles() {
        if (statesLayer) {
            statesLayer.setStyle(stateStyle);
        }
    }

    function zoomToSelection(uf) {
        if (!statesLayer) {
            return;
        }

        if (!uf) {
            map.fitBounds(statesLayer.getBounds(), { padding: [24, 24] });
            return;
        }

        const stateLayer = statesLayer.getLayers().find(layer => getUf(layer.feature) === uf);
        if (stateLayer) {
            map.fitBounds(stateLayer.getBounds(), {
                padding: [24, 24],
                maxZoom: 6,
                minZoom: map.getMinZoom()
            });
        }
    }

    function addDensityLegend() {
        if (densityLegend) {
            densityLegend.remove();
        }

        densityLegend = L.control({ position: "topleft" });
        densityLegend.onAdd = () => {
            const container = L.DomUtil.create("div", "cnes-density-legend");
            const title = document.createElement("strong");
            title.textContent = "Sindicatos por estado";
            container.appendChild(title);

            const total = Object.values(countsByUf).reduce((sum, count) => sum + count, 0);
            const summary = document.createElement("div");
            summary.className = "cnes-density-summary";
            summary.textContent = `${total.toLocaleString("pt-BR")} entidades na base`;
            container.appendChild(summary);

            const scale = document.createElement("div");
            scale.className = "cnes-density-scale";
            const ramp = document.createElement("div");
            ramp.className = "cnes-density-ramp";
            const scaleLabels = document.createElement("div");
            scaleLabels.className = "cnes-density-scale-labels";
            const minLabel = document.createElement("span");
            minLabel.textContent = "Menos";
            const maxLabel = document.createElement("span");
            maxLabel.textContent = "Mais";
            scaleLabels.append(minLabel, maxLabel);
            scale.append(ramp, scaleLabels);
            container.appendChild(scale);

            let lowerBound = 1;
            densityBreaks.forEach((upperBound, index) => {
                const label = lowerBound === upperBound
                    ? upperBound.toLocaleString("pt-BR")
                    : `${lowerBound.toLocaleString("pt-BR")}–${upperBound.toLocaleString("pt-BR")}`;
                addLegendRow(container, densityColors[index], label);
                lowerBound = upperBound + 1;
            });
            addLegendRow(container, noDataColor, "Sem entidades");
            return container;
        };
        densityLegend.addTo(map);
    }

    function addLegendRow(container, color, label) {
        const row = document.createElement("div");
        row.className = "cnes-density-row";

        const swatch = document.createElement("span");
        swatch.className = "cnes-density-swatch";
        swatch.style.backgroundColor = color;

        const text = document.createElement("span");
        text.textContent = label;
        row.append(swatch, text);
        container.appendChild(row);
    }

    return {
        async init(elementId, reference, isDark, counts) {
            const container = document.getElementById(elementId);
            if (!container || !window.L) {
                throw new Error("O contêiner do mapa ou a biblioteca Leaflet não foi carregado.");
            }

            dotNetReference = reference;
            darkTheme = isDark;
            document.documentElement.setAttribute("data-theme", darkTheme ? "dark" : "light");
            countsByUf = Object.fromEntries(
                Object.entries(counts ?? {}).map(([uf, count]) => [uf.toUpperCase(), Number(count)])
            );
            calculateDensityBreaks();
            container.classList.toggle("cnes-map-dark", darkTheme);
            map = L.map(container, {
                scrollWheelZoom: true,
                maxBoundsViscosity: 1
            }).setView([-14.2, -51.9], 4);

            let geoJson;
            try {
                const response = await fetch(statesUrl);
                if (!response.ok) {
                    throw new Error(`resposta HTTP ${response.status}`);
                }
                geoJson = await response.json();
            } catch (error) {
                console.error("Falha ao carregar os limites dos estados para o mapa.", error);
                const notice = document.createElement("div");
                notice.setAttribute("role", "alert");
                notice.textContent = "Não foi possível carregar os limites dos estados. A busca e os filtros continuam disponíveis.";
                Object.assign(notice.style, {
                    position: "absolute",
                    zIndex: "1000",
                    top: "1rem",
                    left: "1rem",
                    right: "1rem",
                    padding: "0.75rem",
                    color: "#7f1d1d",
                    background: "#fef2f2",
                    border: "1px solid #ef4444",
                    borderRadius: "6px"
                });
                container.appendChild(notice);
                return;
            }

            statesLayer = L.geoJSON(geoJson, {
                style: stateStyle,
                onEachFeature(feature, layer) {
                    const properties = feature.properties;
                    const uf = getUf(feature);
                    const stateName = properties.name ?? properties.name_pt ?? uf;
                    if (stateName) {
                        const tooltip = document.createElement("span");
                        tooltip.textContent = `${stateName}${uf ? ` (${uf})` : ""}: ${getCount(feature).toLocaleString("pt-BR")} sindicatos`;
                        layer.bindTooltip(tooltip);
                    }
                    layer.on("click", () => {
                        if (uf) {
                            selectedUf = selectedUf === uf ? "" : uf;
                            refreshStateStyles();
                            zoomToSelection(selectedUf);
                            dotNetReference.invokeMethodAsync("OnEstadoClicadoNoMapa", selectedUf)
                                .catch(error => console.error("Não foi possível sincronizar a UF selecionada com a tela.", error));
                        }
                    });
                }
            }).addTo(map);
            const brazilBounds = statesLayer.getBounds();
            map.setMaxBounds(brazilBounds.pad(0.06));
            map.setMinZoom(map.getBoundsZoom(brazilBounds.pad(0.06)));
            map.fitBounds(brazilBounds, { padding: [24, 24] });
            addDensityLegend();
            requestAnimationFrame(() => map.invalidateSize());
        },

        selectUfFromBlazor(uf) {
            selectedUf = (uf ?? "").toUpperCase();
            refreshStateStyles();
            zoomToSelection(selectedUf);
        },

        setTheme(isDark) {
            darkTheme = isDark;
            document.documentElement.setAttribute("data-theme", darkTheme ? "dark" : "light");
            if (map) {
                map.getContainer().classList.toggle("cnes-map-dark", darkTheme);
            }
            refreshStateStyles();
        }
    };
})();
