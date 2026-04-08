window.sdpGraph = {
  makeDraggableById: (elementId, dotnetRef, serviceId) => {
    const element = document.getElementById(elementId);
    if (!element || element.dataset.dragReady === '1') return;
    element.dataset.dragReady = '1';

    let isDown = false;
    let offsetX = 0;
    let offsetY = 0;

    element.addEventListener('mousedown', (e) => {
      isDown = true;
      offsetX = e.offsetX;
      offsetY = e.offsetY;
      element.style.cursor = 'grabbing';
    });

    window.addEventListener('mousemove', (e) => {
      if (!isDown) return;
      const parentRect = element.parentElement.getBoundingClientRect();
      const x = e.clientX - parentRect.left - offsetX;
      const y = e.clientY - parentRect.top - offsetY;
      element.style.left = `${x}px`;
      element.style.top = `${y}px`;
    });

    window.addEventListener('mouseup', async () => {
      if (!isDown) return;
      isDown = false;
      element.style.cursor = 'grab';
      const x = parseFloat(element.style.left || '0');
      const y = parseFloat(element.style.top || '0');
      await dotnetRef.invokeMethodAsync('OnNodeDragged', serviceId, x, y);
    });
  }
};
