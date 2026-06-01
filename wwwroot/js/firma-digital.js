// ============================================
// FIRMA DIGITAL SCL
// ============================================

document.addEventListener("DOMContentLoaded", () => {

    const canvas = document.getElementById("signature-pad");

    if (!canvas)
        return;

    const ctx = canvas.getContext("2d");

    const inputFirma =
        document.getElementById("FirmaBase64");

    const btnLimpiar =
        document.getElementById("btnLimpiarFirma");

    const form =
        document.getElementById("formRegistro");

    const firmaError =
        document.getElementById("firmaError");

    let dibujando = false;
    let firmaRealizada = false;

    // ============================================
    // AJUSTAR CANVAS
    // ============================================

    function resizeCanvas() {

        const ratio =
            Math.max(window.devicePixelRatio || 1, 1);

        const rect =
            canvas.getBoundingClientRect();

        canvas.width =
            rect.width * ratio;

        canvas.height =
            rect.height * ratio;

        ctx.scale(ratio, ratio);

        ctx.lineWidth = 2.5;
        ctx.lineCap = "round";
        ctx.lineJoin = "round";
        ctx.strokeStyle = "#000";

    }

    resizeCanvas();

    window.addEventListener(
        "resize",
        resizeCanvas
    );

    // ============================================
    // OBTENER POSICIÓN
    // ============================================

    function getPosition(e) {

        const rect =
            canvas.getBoundingClientRect();

        if (e.touches && e.touches.length > 0) {

            return {
                x: e.touches[0].clientX - rect.left,
                y: e.touches[0].clientY - rect.top
            };
        }

        return {
            x: e.clientX - rect.left,
            y: e.clientY - rect.top
        };
    }

    // ============================================
    // INICIAR DIBUJO
    // ============================================

    function startDraw(e) {

        e.preventDefault();

        dibujando = true;
        firmaRealizada = true;

        firmaError.classList.add("d-none");

        const pos =
            getPosition(e);

        ctx.beginPath();

        ctx.moveTo(
            pos.x,
            pos.y
        );
    }

    // ============================================
    // DIBUJAR
    // ============================================

    function draw(e) {

        if (!dibujando)
            return;

        e.preventDefault();

        const pos =
            getPosition(e);

        ctx.lineTo(
            pos.x,
            pos.y
        );

        ctx.stroke();
    }

    // ============================================
    // FINALIZAR
    // ============================================

    function stopDraw(e) {

        if (!dibujando)
            return;

        e.preventDefault();

        dibujando = false;
        ctx.closePath();
    }

    // ============================================
    // MOUSE
    // ============================================

    canvas.addEventListener(
        "mousedown",
        startDraw
    );

    canvas.addEventListener(
        "mousemove",
        draw
    );

    canvas.addEventListener(
        "mouseup",
        stopDraw
    );

    canvas.addEventListener(
        "mouseleave",
        stopDraw
    );

    // ============================================
    // TOUCH
    // ============================================

    canvas.addEventListener(
        "touchstart",
        startDraw,
        { passive: false }
    );

    canvas.addEventListener(
        "touchmove",
        draw,
        { passive: false }
    );

    canvas.addEventListener(
        "touchend",
        stopDraw,
        { passive: false }
    );

    // ============================================
    // LIMPIAR FIRMA
    // ============================================

    btnLimpiar.addEventListener(
        "click",
        () => {

            ctx.clearRect(
                0,
                0,
                canvas.width,
                canvas.height
            );

            firmaRealizada = false;

            inputFirma.value = "";

            firmaError.classList.add(
                "d-none"
            );
        }
    );

    // ============================================
    // CONVERTIR A BASE64 COMPRIMIDO
    // ============================================

    function obtenerFirmaComprimida() {

        const canvasTemp =
            document.createElement("canvas");

        const tempCtx =
            canvasTemp.getContext("2d");

        canvasTemp.width = 560;
        canvasTemp.height = 260;

        tempCtx.fillStyle = "#FFFFFF";

        tempCtx.fillRect(
            0,
            0,
            canvasTemp.width,
            canvasTemp.height
        );

        tempCtx.drawImage(
            canvas,
            0,
            0,
            canvasTemp.width,
            canvasTemp.height
        );

        return canvasTemp.toDataURL(
            "image/jpeg",
            0.75
        );
    }

    // ============================================
    // VALIDAR ENVÍO
    // ============================================

    form.addEventListener(
        "submit",
        function (e) {

            if (!firmaRealizada) {

                e.preventDefault();

                firmaError.classList.remove(
                    "d-none"
                );

                return;
            }

            const firmaBase64 =
                obtenerFirmaComprimida();

            inputFirma.value =
                firmaBase64;
        }
    );

});