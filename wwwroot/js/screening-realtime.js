/**
 * screening-realtime.js
 * Real-time collaborative screening for JobAppHR ManualProcess.
 * Uses SignalR for broadcasting and AJAX for per-row saves.
 */
(function () {
    'use strict';

    // ── Configuration (set by the page) ─────────────────────────────
    const config = window.ScreeningConfig || {};
    const intakeCode     = config.intakeCode || '';
    const currentStage   = config.currentStage || '';
    const currentStatus  = config.currentStatus || '';
    const currentUserId  = config.currentUserId || '';
    const currentUserName = config.currentUserName || '';
    const hubUrl         = config.hubUrl || '/hubs/screening';
    const saveUrl        = config.saveUrl || '/ManualProcess/UpdateSingleApplicant';
    const antiForgeryToken = config.antiForgeryToken || '';

    if (!intakeCode) {
        console.warn('[ScreeningRT] No intakeCode configured. Real-time disabled.');
        return;
    }

    // ── State ───────────────────────────────────────────────────────
    let connection = null;
    let isConnected = false;
    const lockedRows = {};          // applicationCode -> { userId, userName }
    const pendingSaves = {};        // applicationCode -> timeout handle
    const LOCK_TIMEOUT_MS = 30000; // 30 seconds auto-unlock
    const SAVE_DEBOUNCE_MS = 600;  // debounce rapid changes

    // ── SignalR Connection ──────────────────────────────────────────
    function initConnection() {
        connection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl)
            .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
            .configureLogging(signalR.LogLevel.Warning)
            .build();

        // ─ Receive handlers ─
        connection.on('ReceiveStatusUpdate', onReceiveStatusUpdate);
        connection.on('ReceiveRowLock', onReceiveRowLock);
        connection.on('ReceiveRowUnlock', onReceiveRowUnlock);
        connection.on('ReceiveLockRejected', onReceiveLockRejected);
        connection.on('ReceiveActiveUsers', onReceiveActiveUsers);
        connection.on('ReceiveCurrentLocks', onReceiveCurrentLocks);

        // ─ Connection lifecycle ─
        connection.onreconnecting(() => setConnectionStatus('reconnecting'));
        connection.onreconnected(() => {
            setConnectionStatus('connected');
            joinGroup();
        });
        connection.onclose(() => setConnectionStatus('disconnected'));

        startConnection();
    }

    function startConnection() {
        setConnectionStatus('connecting');
        connection.start()
            .then(() => {
                isConnected = true;
                setConnectionStatus('connected');
                joinGroup();
            })
            .catch(err => {
                console.error('[ScreeningRT] Connection failed:', err);
                setConnectionStatus('disconnected');
                // Retry after 5 seconds
                setTimeout(startConnection, 5000);
            });
    }

    function joinGroup() {
        if (!isConnected) return;
        connection.invoke('JoinIntakeGroup', intakeCode, currentStage, currentStatus)
            .catch(err => console.error('[ScreeningRT] JoinIntakeGroup failed:', err));
    }

    // ── UI: Connection Status ───────────────────────────────────────
    function setConnectionStatus(status) {
        const el = document.getElementById('connectionStatus');
        if (!el) return;

        const dot = el.querySelector('.status-dot');
        const text = el.querySelector('.status-text');

        el.className = 'connection-status status-' + status;
        if (dot) {
            dot.className = 'status-dot dot-' + status;
        }
        if (text) {
            const labels = {
                connecting: 'Connecting...',
                connected: 'Live',
                reconnecting: 'Reconnecting...',
                disconnected: 'Disconnected'
            };
            text.textContent = labels[status] || status;
        }
    }

    // ── Receive: Status Update from another user ────────────────────
    function onReceiveStatusUpdate(applicationCode, newStatus, newRemarks, userId, userName) {
        const row = findRow(applicationCode);
        if (!row) return;

        // Update the select dropdown
        const select = row.querySelector('select[name="CurrentStatus"]');
        if (select) {
            select.value = newStatus;
            // Also update the hidden field
            const hiddenStatus = row.querySelector('input[id^="currentstatus_"]');
            if (hiddenStatus) hiddenStatus.value = newStatus;
            // Update the old status hidden
            const oldStatus = row.querySelector('input[name^="oldstatus_"]');
            if (oldStatus) oldStatus.value = newStatus;
        }

        // Flash the row to indicate remote update
        row.classList.add('row-remote-update');
        setTimeout(() => row.classList.remove('row-remote-update'), 2000);

        // Show toast notification
        showToast(`${userName} changed ${applicationCode} to ${newStatus}`);
    }

    // ── Receive: Row Lock ───────────────────────────────────────────
    function onReceiveRowLock(applicationCode, userId, userName) {
        lockedRows[applicationCode] = { userId, userName };
        const row = findRow(applicationCode);
        if (!row) return;

        row.classList.add('row-locked');

        // Disable controls
        const select = row.querySelector('select[name="CurrentStatus"]');
        const remarksInput = row.querySelector('input[type="text"][id^="newremarks_"]');
        if (select) select.disabled = true;
        if (remarksInput) remarksInput.disabled = true;

        // Add lock badge
        let badge = row.querySelector('.lock-badge');
        if (!badge) {
            badge = document.createElement('span');
            badge.className = 'lock-badge';
            const actionCell = row.querySelector('td:last-child') || row.querySelector('td:nth-last-child(1)');
            if (actionCell) actionCell.appendChild(badge);
        }
        badge.innerHTML = '<i class="fa fa-lock"></i> ' + userName;
        badge.title = 'Being edited by ' + userName;
    }

    // ── Receive: Row Unlock ─────────────────────────────────────────
    function onReceiveRowUnlock(applicationCode) {
        delete lockedRows[applicationCode];
        const row = findRow(applicationCode);
        if (!row) return;

        row.classList.remove('row-locked');

        // Re-enable controls
        const select = row.querySelector('select[name="CurrentStatus"]');
        const remarksInput = row.querySelector('input[type="text"][id^="newremarks_"]');
        if (select) select.disabled = false;
        if (remarksInput) remarksInput.disabled = false;

        // Remove lock badge
        const badge = row.querySelector('.lock-badge');
        if (badge) badge.remove();
    }

    // ── Receive: Lock Rejected ──────────────────────────────────────
    function onReceiveLockRejected(applicationCode, lockedByUserId, lockedByUserName) {
        showToast(`Row ${applicationCode} is being edited by ${lockedByUserName}`, 'warning');
    }

    // ── Receive: Active Users ───────────────────────────────────────
    function onReceiveActiveUsers(userList) {
        const panel = document.getElementById('activeUsersPanel');
        if (!panel) return;

        const container = panel.querySelector('.active-users-list');
        if (!container) return;

        // Filter out current user for display, but count them
        container.innerHTML = '';
        let otherCount = 0;

        userList.forEach(u => {
            const chip = document.createElement('span');
            chip.className = 'user-chip-small';
            if (u.userId === currentUserId) {
                chip.classList.add('user-chip-self');
                chip.innerHTML = '<i class="fa fa-user"></i> You';
            } else {
                otherCount++;
                chip.innerHTML = '<i class="fa fa-user"></i> ' + u.userName;
            }
            container.appendChild(chip);
        });

        // Show/hide the panel
        if (userList.length > 1) {
            panel.classList.remove('d-none');
        } else {
            panel.classList.add('d-none');
        }
    }

    // ── Receive: Current Locks (on join) ────────────────────────────
    function onReceiveCurrentLocks(locks) {
        locks.forEach(l => {
            onReceiveRowLock(l.applicationCode, l.userId, l.userName);
        });
    }

    // ── Send: Lock Row ──────────────────────────────────────────────
    function sendLockRow(applicationCode) {
        if (!isConnected) return;
        connection.invoke('LockRow', intakeCode, currentStage, currentStatus, applicationCode)
            .catch(err => console.error('[ScreeningRT] LockRow failed:', err));
    }

    // ── Send: Unlock Row ────────────────────────────────────────────
    function sendUnlockRow(applicationCode) {
        if (!isConnected) return;
        connection.invoke('UnlockRow', intakeCode, currentStage, currentStatus, applicationCode)
            .catch(err => console.error('[ScreeningRT] UnlockRow failed:', err));
    }

    // ── Send: Broadcast Status Update ───────────────────────────────
    function sendStatusUpdate(applicationCode, newStatus, newRemarks) {
        if (!isConnected) return;
        connection.invoke('UpdateApplicantStatus',
            intakeCode, currentStage, currentStatus,
            applicationCode, newStatus, newRemarks, currentUserId, currentUserName)
            .catch(err => console.error('[ScreeningRT] UpdateApplicantStatus failed:', err));
    }

    // ── AJAX: Per-Row Save ──────────────────────────────────────────
    function saveRow(applicationCode, newStatus, oldStatus, newRemarks) {
        const row = findRow(applicationCode);
        if (!row) return;

        // Show saving indicator
        setSaveIndicator(row, 'saving');

        const payload = {
            applicationCode: applicationCode,
            intakeCode: intakeCode,
            currentStage: 'FINAL',
            previousStage: currentStage,
            newStatus: newStatus,
            oldStatus: oldStatus,
            newRemarks: newRemarks
        };

        fetch(saveUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': antiForgeryToken
            },
            body: JSON.stringify(payload)
        })
        .then(r => r.json())
        .then(result => {
            if (result.success) {
                setSaveIndicator(row, 'saved');
                // Update old status to match new status (for future changes)
                const oldStatusInput = row.querySelector('input[name^="oldstatus_"]');
                if (oldStatusInput) oldStatusInput.value = newStatus;
                // Broadcast to other users
                sendStatusUpdate(applicationCode, newStatus, newRemarks);
            } else {
                setSaveIndicator(row, 'error');
                showToast('Failed to save ' + applicationCode + ': ' + (result.error || 'Unknown error'), 'error');
            }
        })
        .catch(err => {
            setSaveIndicator(row, 'error');
            showToast('Network error saving ' + applicationCode, 'error');
            console.error('[ScreeningRT] Save failed:', err);
        });
    }

    // ── Debounced Save ──────────────────────────────────────────────
    function debouncedSave(applicationCode, newStatus, oldStatus, newRemarks) {
        if (pendingSaves[applicationCode]) {
            clearTimeout(pendingSaves[applicationCode]);
        }
        pendingSaves[applicationCode] = setTimeout(() => {
            delete pendingSaves[applicationCode];
            saveRow(applicationCode, newStatus, oldStatus, newRemarks);
        }, SAVE_DEBOUNCE_MS);
    }

    // ── Save Indicator ──────────────────────────────────────────────
    function setSaveIndicator(row, state) {
        let indicator = row.querySelector('.save-indicator');
        if (!indicator) {
            indicator = document.createElement('span');
            indicator.className = 'save-indicator';
            const statusCell = row.querySelector('td:nth-child(11)'); // Status column
            if (statusCell) statusCell.appendChild(indicator);
        }

        indicator.className = 'save-indicator si-' + state;
        switch (state) {
            case 'saving':
                indicator.innerHTML = '<i class="fa fa-spinner fa-spin"></i>';
                indicator.title = 'Saving...';
                break;
            case 'saved':
                indicator.innerHTML = '<i class="fa fa-check"></i>';
                indicator.title = 'Saved';
                // Auto-clear after 3 seconds
                setTimeout(() => {
                    if (indicator.classList.contains('si-saved')) {
                        indicator.innerHTML = '';
                        indicator.className = 'save-indicator';
                    }
                }, 3000);
                break;
            case 'error':
                indicator.innerHTML = '<i class="fa fa-exclamation-triangle"></i>';
                indicator.title = 'Save failed — click Save All to retry';
                break;
        }
    }

    // ── Row Focus/Blur (Lock Management) ────────────────────────────
    const rowLockTimers = {}; // applicationCode -> timeout handle

    function onRowControlFocus(applicationCode) {
        // Clear any pending unlock
        if (rowLockTimers[applicationCode]) {
            clearTimeout(rowLockTimers[applicationCode]);
            delete rowLockTimers[applicationCode];
        }
        sendLockRow(applicationCode);

        // Safety: auto-unlock after timeout
        rowLockTimers[applicationCode + '_safety'] = setTimeout(() => {
            sendUnlockRow(applicationCode);
        }, LOCK_TIMEOUT_MS);
    }

    function onRowControlBlur(applicationCode) {
        // Clear safety timer
        if (rowLockTimers[applicationCode + '_safety']) {
            clearTimeout(rowLockTimers[applicationCode + '_safety']);
            delete rowLockTimers[applicationCode + '_safety'];
        }
        // Delay unlock slightly to allow tab between select and remarks
        rowLockTimers[applicationCode] = setTimeout(() => {
            sendUnlockRow(applicationCode);
            delete rowLockTimers[applicationCode];
        }, 300);
    }

    // ── Wire Up Events ──────────────────────────────────────────────
    function wireRowEvents() {
        const rows = document.querySelectorAll('#datagrid tbody tr[data-app-code]');

        rows.forEach(row => {
            const applicationCode = row.getAttribute('data-app-code');
            if (!applicationCode) return;

            const select = row.querySelector('select[name="CurrentStatus"]');
            const remarksInput = row.querySelector('input[type="text"][id^="newremarks_"]');

            // ─ Focus/Blur for locking ─
            if (select) {
                select.addEventListener('focus', () => onRowControlFocus(applicationCode));
                select.addEventListener('blur', () => onRowControlBlur(applicationCode));

                // ─ Status change → auto-save ─
                select.addEventListener('change', function () {
                    const newStatus = this.value;
                    const hiddenStatus = row.querySelector('input[id^="currentstatus_"]');
                    if (hiddenStatus) hiddenStatus.value = newStatus;

                    const oldStatusInput = row.querySelector('input[name^="oldstatus_"]');
                    const oldStatus = oldStatusInput ? oldStatusInput.value : '';

                    const remarksEl = row.querySelector('input[type="text"][id^="newremarks_"]');
                    const newRemarks = remarksEl ? remarksEl.value : '';

                    debouncedSave(applicationCode, newStatus, oldStatus, newRemarks);
                });
            }

            if (remarksInput) {
                remarksInput.addEventListener('focus', () => onRowControlFocus(applicationCode));
                remarksInput.addEventListener('blur', () => {
                    onRowControlBlur(applicationCode);

                    // Save on blur if remarks were changed
                    const remarksValue = remarksInput.value.trim();
                    if (remarksValue) {
                        const selectEl = row.querySelector('select[name="CurrentStatus"]');
                        const newStatus = selectEl ? selectEl.value : '';
                        const oldStatusInput = row.querySelector('input[name^="oldstatus_"]');
                        const oldStatus = oldStatusInput ? oldStatusInput.value : '';
                        debouncedSave(applicationCode, newStatus, oldStatus, remarksValue);
                    }
                });
            }
        });
    }

    // ── Helper: Find Row by ApplicationCode ─────────────────────────
    function findRow(applicationCode) {
        return document.querySelector('#datagrid tbody tr[data-app-code="' + applicationCode + '"]');
    }

    // ── Toast Notifications ─────────────────────────────────────────
    function showToast(message, type) {
        type = type || 'info';
        let container = document.getElementById('screeningToasts');
        if (!container) {
            container = document.createElement('div');
            container.id = 'screeningToasts';
            container.className = 'screening-toasts';
            document.body.appendChild(container);
        }

        const toast = document.createElement('div');
        toast.className = 'screening-toast toast-' + type;
        toast.textContent = message;
        container.appendChild(toast);

        // Animate in
        requestAnimationFrame(() => toast.classList.add('toast-show'));

        // Remove after 4 seconds
        setTimeout(() => {
            toast.classList.remove('toast-show');
            toast.classList.add('toast-hide');
            setTimeout(() => toast.remove(), 300);
        }, 4000);
    }

    // ── Page Unload: Release Locks ──────────────────────────────────
    function onBeforeUnload() {
        if (isConnected) {
            connection.invoke('LeaveIntakeGroup', intakeCode, currentStage, currentStatus)
                .catch(() => {});
        }
    }

    // ── Initialization ──────────────────────────────────────────────
    function init() {
        // Wait for DataTables to finish rendering
        // The page uses DataTables which may re-render the tbody
        const checkReady = setInterval(() => {
            const rows = document.querySelectorAll('#datagrid tbody tr[data-app-code]');
            if (rows.length > 0) {
                clearInterval(checkReady);
                wireRowEvents();
                initConnection();
                window.addEventListener('beforeunload', onBeforeUnload);
            }
        }, 200);

        // Timeout after 15 seconds
        setTimeout(() => clearInterval(checkReady), 15000);
    }

    // Start when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();
