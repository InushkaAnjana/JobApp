// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

(function () {
	const storageKey = 'jobapphr.uiTheme';
	const defaults = {
		shell: 'sky',
		background: 'default',
		font: 'inter',
		accent: 'blue',
		size: 'md',
	};

	const shellTokens = {
		sky: {
			navbarBg: 'linear-gradient(135deg, #DFF0FF, #CFE7FF)',
			navbarText: '#163A63',
			navbarHoverBg: 'rgba(255, 255, 255, 0.3)',
			navbarBorder: 'rgba(96, 165, 250, 0.16)',
			settingsBg: 'rgba(255, 255, 255, 0.42)',
			settingsBorder: 'rgba(96, 165, 250, 0.2)',
			settingsText: '#163A63',
			settingsIconBg: 'rgba(96, 165, 250, 0.18)',
			userChipBg: 'rgba(255, 255, 255, 0.4)',
			userChipBorder: 'rgba(96, 165, 250, 0.18)',
			userChipText: '#163A63',
			userChipSubText: 'rgba(22, 58, 99, 0.72)',
			pageSurfaceBg: 'linear-gradient(180deg, rgba(235, 245, 255, 0.98), rgba(247, 251, 255, 0.98))',
			pageSurfaceBorder: 'rgba(96, 165, 250, 0.14)',
			pageSurfaceShadow: '0 18px 42px -28px rgba(59, 130, 246, 0.32)',
			heroBg: 'linear-gradient(135deg, rgba(226, 241, 255, 0.98), rgba(240, 248, 255, 0.98))',
			heroBorder: 'rgba(96, 165, 250, 0.16)',
			heroShadow: '0 18px 44px -30px rgba(59, 130, 246, 0.32)',
			heroText: '#1E293B',
			heroTextMuted: '#475569',
			heroPanelBg: 'rgba(255, 255, 255, 0.82)',
			heroPanelBorder: 'rgba(96, 165, 250, 0.16)',
			heroStatBg: 'rgba(255, 255, 255, 0.94)',
			heroStatBorder: 'rgba(191, 219, 254, 0.9)',
			cardIconBg: 'linear-gradient(135deg, rgba(96, 165, 250, 0.12), rgba(191, 219, 254, 0.18))',
			cardIconColor: '#1D4E89',
			cardHoverBorder: 'rgba(96, 165, 250, 0.22)',
			cardFooterColor: '#2563EB',
		},
		white: {
			navbarBg: 'linear-gradient(135deg, #FFFFFF, #F8FAFC)',
			navbarText: '#1E293B',
			navbarHoverBg: 'rgba(148, 163, 184, 0.12)',
			navbarBorder: 'rgba(148, 163, 184, 0.18)',
			settingsBg: 'rgba(255, 255, 255, 0.72)',
			settingsBorder: 'rgba(148, 163, 184, 0.22)',
			settingsText: '#1E293B',
			settingsIconBg: 'rgba(148, 163, 184, 0.12)',
			userChipBg: 'rgba(255, 255, 255, 0.72)',
			userChipBorder: 'rgba(148, 163, 184, 0.22)',
			userChipText: '#1E293B',
			userChipSubText: 'rgba(30, 41, 59, 0.7)',
			pageSurfaceBg: 'linear-gradient(180deg, rgba(255, 255, 255, 0.98), rgba(248, 250, 252, 0.98))',
			pageSurfaceBorder: 'rgba(148, 163, 184, 0.18)',
			pageSurfaceShadow: '0 18px 42px -28px rgba(15, 23, 42, 0.28)',
			heroBg: 'linear-gradient(135deg, rgba(255, 255, 255, 0.98), rgba(248, 250, 252, 0.98))',
			heroBorder: 'rgba(148, 163, 184, 0.18)',
			heroShadow: '0 18px 44px -30px rgba(15, 23, 42, 0.28)',
			heroText: '#1E293B',
			heroTextMuted: '#475569',
			heroPanelBg: 'rgba(255, 255, 255, 0.94)',
			heroPanelBorder: 'rgba(226, 232, 240, 0.9)',
			heroStatBg: '#FFFFFF',
			heroStatBorder: 'rgba(226, 232, 240, 0.9)',
			cardIconBg: 'linear-gradient(135deg, rgba(148, 163, 184, 0.12), rgba(226, 232, 240, 0.2))',
			cardIconColor: '#475569',
			cardHoverBorder: 'rgba(148, 163, 184, 0.24)',
			cardFooterColor: '#475569',
		},
		ice: {
			navbarBg: 'linear-gradient(135deg, #F5FAFF, #EDF6FF)',
			navbarText: '#163A63',
			navbarHoverBg: 'rgba(59, 130, 246, 0.08)',
			navbarBorder: 'rgba(147, 197, 253, 0.24)',
			settingsBg: 'rgba(255, 255, 255, 0.58)',
			settingsBorder: 'rgba(147, 197, 253, 0.22)',
			settingsText: '#163A63',
			settingsIconBg: 'rgba(191, 219, 254, 0.18)',
			userChipBg: 'rgba(255, 255, 255, 0.58)',
			userChipBorder: 'rgba(147, 197, 253, 0.22)',
			userChipText: '#163A63',
			userChipSubText: 'rgba(22, 58, 99, 0.72)',
			pageSurfaceBg: 'linear-gradient(180deg, rgba(248, 252, 255, 0.98), rgba(255, 255, 255, 0.98))',
			pageSurfaceBorder: 'rgba(191, 219, 254, 0.2)',
			pageSurfaceShadow: '0 18px 42px -28px rgba(59, 130, 246, 0.24)',
			heroBg: 'linear-gradient(135deg, rgba(241, 248, 255, 0.98), rgba(255, 255, 255, 0.98))',
			heroBorder: 'rgba(191, 219, 254, 0.22)',
			heroShadow: '0 18px 44px -30px rgba(59, 130, 246, 0.24)',
			heroText: '#1E293B',
			heroTextMuted: '#475569',
			heroPanelBg: 'rgba(255, 255, 255, 0.88)',
			heroPanelBorder: 'rgba(191, 219, 254, 0.2)',
			heroStatBg: 'rgba(255, 255, 255, 0.96)',
			heroStatBorder: 'rgba(191, 219, 254, 0.84)',
			cardIconBg: 'linear-gradient(135deg, rgba(96, 165, 250, 0.12), rgba(191, 219, 254, 0.18))',
			cardIconColor: '#2563EB',
			cardHoverBorder: 'rgba(96, 165, 250, 0.2)',
			cardFooterColor: '#2563EB',
		},
		mist: {
			navbarBg: 'linear-gradient(135deg, #F8FAFC, #E2E8F0)',
			navbarText: '#1E293B',
			navbarHoverBg: 'rgba(148, 163, 184, 0.14)',
			navbarBorder: 'rgba(148, 163, 184, 0.18)',
			settingsBg: 'rgba(255, 255, 255, 0.72)',
			settingsBorder: 'rgba(148, 163, 184, 0.2)',
			settingsText: '#1E293B',
			settingsIconBg: 'rgba(148, 163, 184, 0.12)',
			userChipBg: 'rgba(255, 255, 255, 0.7)',
			userChipBorder: 'rgba(148, 163, 184, 0.2)',
			userChipText: '#1E293B',
			userChipSubText: 'rgba(30, 41, 59, 0.7)',
			pageSurfaceBg: 'linear-gradient(180deg, rgba(250, 252, 255, 0.98), rgba(255, 255, 255, 0.98))',
			pageSurfaceBorder: 'rgba(148, 163, 184, 0.16)',
			pageSurfaceShadow: '0 18px 42px -28px rgba(15, 23, 42, 0.26)',
			heroBg: 'linear-gradient(135deg, rgba(249, 250, 251, 0.98), rgba(255, 255, 255, 0.98))',
			heroBorder: 'rgba(148, 163, 184, 0.18)',
			heroShadow: '0 18px 44px -30px rgba(15, 23, 42, 0.26)',
			heroText: '#1E293B',
			heroTextMuted: '#475569',
			heroPanelBg: 'rgba(255, 255, 255, 0.92)',
			heroPanelBorder: 'rgba(148, 163, 184, 0.18)',
			heroStatBg: '#FFFFFF',
			heroStatBorder: 'rgba(226, 232, 240, 0.9)',
			cardIconBg: 'linear-gradient(135deg, rgba(148, 163, 184, 0.12), rgba(226, 232, 240, 0.18))',
			cardIconColor: '#475569',
			cardHoverBorder: 'rgba(148, 163, 184, 0.22)',
			cardFooterColor: '#475569',
		},
	};

	const themeTokens = {
		background: {
			default: {
				bgSecondary: '#F6F8FC',
				bgPrimary: '#FFFFFF',
				bgImageLayer: 'radial-gradient(circle at top left, rgba(0, 86, 179, 0.08), transparent 28%), radial-gradient(circle at bottom right, rgba(64, 168, 58, 0.08), transparent 24%)',
				surfaceStrong: 'rgba(255, 255, 255, 0.98)',
				surfaceBorder: 'rgba(148, 163, 184, 0.18)',
			},
			calm: {
				bgSecondary: '#F8FAFC',
				bgPrimary: '#FFFFFF',
				bgImageLayer: 'linear-gradient(135deg, rgba(248, 250, 252, 0.92), rgba(226, 232, 240, 0.92))',
				surfaceStrong: 'rgba(255, 255, 255, 0.98)',
				surfaceBorder: 'rgba(148, 163, 184, 0.18)',
			},
			night: {
				bgSecondary: '#0B1120',
				bgPrimary: '#111827',
				bgImageLayer: 'radial-gradient(circle at top left, rgba(59, 130, 246, 0.2), transparent 30%), radial-gradient(circle at bottom right, rgba(16, 185, 129, 0.14), transparent 26%), linear-gradient(135deg, #0B1120, #111827)',
				surfaceStrong: 'rgba(17, 24, 39, 0.94)',
				surfaceBorder: 'rgba(148, 163, 184, 0.16)',
				textPrimary: '#E5E7EB',
				textSecondary: '#B6C2D2',
			},
			slate: {
				bgSecondary: '#F1F5F9',
				bgPrimary: '#FFFFFF',
				bgImageLayer: 'radial-gradient(circle at top right, rgba(15, 23, 42, 0.12), transparent 28%), linear-gradient(135deg, rgba(241, 245, 249, 0.98), rgba(226, 232, 240, 0.98))',
				surfaceStrong: 'rgba(255, 255, 255, 0.98)',
				surfaceBorder: 'rgba(148, 163, 184, 0.18)',
			},
		},
		font: {
			inter: '"Inter", -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif',
			jakarta: '"Plus Jakarta Sans", sans-serif',
			source: '"Source Sans 3", sans-serif',
			grotesk: '"Space Grotesk", sans-serif',
		},
		accent: {
			blue: ['#0056B3', '#2F7BDB', '#003C7C'],
			emerald: ['#0F766E', '#14B8A6', '#115E59'],
			graphite: ['#334155', '#475569', '#1E293B'],
			indigo: ['#4F46E5', '#6366F1', '#3730A3'],
		},
		size: {
			sm: '0.94',
			md: '1',
			lg: '1.08',
		}
	};

	function loadTheme() {
		try {
			const stored = JSON.parse(localStorage.getItem(storageKey) || '{}');
			return { ...defaults, ...stored };
		} catch {
			return { ...defaults };
		}
	}

	function saveTheme(theme) {
		localStorage.setItem(storageKey, JSON.stringify(theme));
	}

	function applyTheme(theme) {
		const root = document.documentElement;
		const body = document.body;
		if (!body) {
			return;
		}

		const backgroundTokens = themeTokens.background[theme.background] || themeTokens.background.default;
		const fontToken = themeTokens.font[theme.font] || themeTokens.font.inter;
		const accentToken = themeTokens.accent[theme.accent] || themeTokens.accent.blue;
		const sizeToken = themeTokens.size[theme.size] || themeTokens.size.md;
		const shellTokensActive = shellTokens[theme.shell] || shellTokens.sky;

		root.style.setProperty('--app-font-family', fontToken);
		root.style.setProperty('--ui-scale', sizeToken);
		root.style.setProperty('--bg-secondary', backgroundTokens.bgSecondary || '#F6F8FC');
		root.style.setProperty('--bg-primary', backgroundTokens.bgPrimary || '#FFFFFF');
		root.style.setProperty('--bg-image-layer', backgroundTokens.bgImageLayer || 'none');
		root.style.setProperty('--surface-strong', backgroundTokens.surfaceStrong || 'rgba(255, 255, 255, 0.98)');
		root.style.setProperty('--surface-border', backgroundTokens.surfaceBorder || 'rgba(148, 163, 184, 0.18)');
		root.style.setProperty('--text-primary', backgroundTokens.textPrimary || '#1E293B');
		root.style.setProperty('--text-secondary', backgroundTokens.textSecondary || '#475569');
		root.style.setProperty('--primary-blue', accentToken[0]);
		root.style.setProperty('--primary-blue-light', accentToken[1]);
		root.style.setProperty('--primary-blue-dark', accentToken[2]);
		root.style.setProperty('--admin-navbar-bg', shellTokensActive.navbarBg);
		root.style.setProperty('--admin-navbar-text', shellTokensActive.navbarText);
		root.style.setProperty('--admin-navbar-hover-bg', shellTokensActive.navbarHoverBg);
		root.style.setProperty('--admin-navbar-border', shellTokensActive.navbarBorder);
		root.style.setProperty('--admin-settings-bg', shellTokensActive.settingsBg);
		root.style.setProperty('--admin-settings-border', shellTokensActive.settingsBorder);
		root.style.setProperty('--admin-settings-text', shellTokensActive.settingsText);
		root.style.setProperty('--admin-settings-icon-bg', shellTokensActive.settingsIconBg);
		root.style.setProperty('--admin-user-chip-bg', shellTokensActive.userChipBg);
		root.style.setProperty('--admin-user-chip-border', shellTokensActive.userChipBorder);
		root.style.setProperty('--admin-user-chip-text', shellTokensActive.userChipText);
		root.style.setProperty('--admin-user-chip-subtext', shellTokensActive.userChipSubText);
		root.style.setProperty('--admin-page-surface-bg', shellTokensActive.pageSurfaceBg);
		root.style.setProperty('--admin-page-surface-border', shellTokensActive.pageSurfaceBorder);
		root.style.setProperty('--admin-page-surface-shadow', shellTokensActive.pageSurfaceShadow);
		root.style.setProperty('--admin-hero-bg', shellTokensActive.heroBg);
		root.style.setProperty('--admin-hero-border', shellTokensActive.heroBorder);
		root.style.setProperty('--admin-hero-shadow', shellTokensActive.heroShadow);
		root.style.setProperty('--admin-hero-text', shellTokensActive.heroText);
		root.style.setProperty('--admin-hero-text-muted', shellTokensActive.heroTextMuted);
		root.style.setProperty('--admin-hero-panel-bg', shellTokensActive.heroPanelBg);
		root.style.setProperty('--admin-hero-panel-border', shellTokensActive.heroPanelBorder);
		root.style.setProperty('--admin-hero-stat-bg', shellTokensActive.heroStatBg);
		root.style.setProperty('--admin-hero-stat-border', shellTokensActive.heroStatBorder);
		root.style.setProperty('--admin-card-icon-bg', shellTokensActive.cardIconBg);
		root.style.setProperty('--admin-card-icon-color', shellTokensActive.cardIconColor);
		root.style.setProperty('--admin-card-hover-border', shellTokensActive.cardHoverBorder);
		root.style.setProperty('--admin-card-footer-color', shellTokensActive.cardFooterColor);

		body.style.fontFamily = fontToken;
		body.style.backgroundColor = backgroundTokens.bgSecondary || '#F6F8FC';
		body.style.backgroundImage = backgroundTokens.bgImageLayer || 'none';
		body.style.color = backgroundTokens.textPrimary || '#1E293B';
		body.style.setProperty('--bg-secondary', backgroundTokens.bgSecondary || '#F6F8FC');
		body.style.setProperty('--bg-primary', backgroundTokens.bgPrimary || '#FFFFFF');
		body.style.setProperty('--surface-strong', backgroundTokens.surfaceStrong || 'rgba(255, 255, 255, 0.98)');
		body.style.setProperty('--surface-border', backgroundTokens.surfaceBorder || 'rgba(148, 163, 184, 0.18)');
		body.style.setProperty('--text-primary', backgroundTokens.textPrimary || '#1E293B');
		body.style.setProperty('--text-secondary', backgroundTokens.textSecondary || '#475569');
		body.style.setProperty('--admin-navbar-bg', shellTokensActive.navbarBg);
		body.style.setProperty('--admin-navbar-text', shellTokensActive.navbarText);
		body.style.setProperty('--admin-navbar-hover-bg', shellTokensActive.navbarHoverBg);
		body.style.setProperty('--admin-navbar-border', shellTokensActive.navbarBorder);
		body.style.setProperty('--admin-settings-bg', shellTokensActive.settingsBg);
		body.style.setProperty('--admin-settings-border', shellTokensActive.settingsBorder);
		body.style.setProperty('--admin-settings-text', shellTokensActive.settingsText);
		body.style.setProperty('--admin-settings-icon-bg', shellTokensActive.settingsIconBg);
		body.style.setProperty('--admin-user-chip-bg', shellTokensActive.userChipBg);
		body.style.setProperty('--admin-user-chip-border', shellTokensActive.userChipBorder);
		body.style.setProperty('--admin-user-chip-text', shellTokensActive.userChipText);
		body.style.setProperty('--admin-user-chip-subtext', shellTokensActive.userChipSubText);
		body.style.setProperty('--admin-page-surface-bg', shellTokensActive.pageSurfaceBg);
		body.style.setProperty('--admin-page-surface-border', shellTokensActive.pageSurfaceBorder);
		body.style.setProperty('--admin-page-surface-shadow', shellTokensActive.pageSurfaceShadow);
		body.style.setProperty('--admin-hero-bg', shellTokensActive.heroBg);
		body.style.setProperty('--admin-hero-border', shellTokensActive.heroBorder);
		body.style.setProperty('--admin-hero-shadow', shellTokensActive.heroShadow);
		body.style.setProperty('--admin-hero-text', shellTokensActive.heroText);
		body.style.setProperty('--admin-hero-text-muted', shellTokensActive.heroTextMuted);
		body.style.setProperty('--admin-hero-panel-bg', shellTokensActive.heroPanelBg);
		body.style.setProperty('--admin-hero-panel-border', shellTokensActive.heroPanelBorder);
		body.style.setProperty('--admin-hero-stat-bg', shellTokensActive.heroStatBg);
		body.style.setProperty('--admin-hero-stat-border', shellTokensActive.heroStatBorder);
		body.style.setProperty('--admin-card-icon-bg', shellTokensActive.cardIconBg);
		body.style.setProperty('--admin-card-icon-color', shellTokensActive.cardIconColor);
		body.style.setProperty('--admin-card-hover-border', shellTokensActive.cardHoverBorder);
		body.style.setProperty('--admin-card-footer-color', shellTokensActive.cardFooterColor);

		const preview = document.querySelector('[data-theme-preview]');
		if (preview) {
			// Always use the accent gradient as the preview base so contrast is guaranteed
			const previewBg = 'linear-gradient(135deg, ' + accentToken[0] + ', ' + accentToken[2] + ')';
			preview.style.setProperty('--preview-shell-bg', previewBg);
			preview.style.setProperty('--preview-shell-border', 'transparent');
			preview.style.setProperty('--preview-bg-image-layer', 'none');
			preview.style.setProperty('--preview-primary-blue', accentToken[0]);
			preview.style.setProperty('--preview-primary-blue-light', accentToken[1]);
			preview.style.setProperty('--preview-primary-blue-dark', accentToken[2]);
			preview.style.setProperty('--preview-font-family', fontToken);
			// Accent gradient is always dark enough — use white overlays for elements
			preview.style.setProperty('--preview-text-primary', '#FFFFFF');
			preview.style.setProperty('--preview-text-secondary', 'rgba(255, 255, 255, 0.78)');
			preview.style.setProperty('--preview-line-color', 'rgba(255, 255, 255, 0.28)');
			preview.style.setProperty('--preview-pill-bg', 'rgba(255, 255, 255, 0.18)');
			preview.style.setProperty('--preview-tile-bg', 'rgba(255, 255, 255, 0.15)');
			preview.style.setProperty('--preview-shell-text', '#FFFFFF');
			preview.style.setProperty('--preview-shell-muted', 'rgba(255, 255, 255, 0.78)');
			preview.style.setProperty('--preview-shell-panel-bg', 'rgba(255, 255, 255, 0.15)');
			preview.style.setProperty('--preview-shell-panel-border', 'rgba(255, 255, 255, 0.18)');
		}

		document.querySelectorAll('[data-theme-group]').forEach((group) => {
			const key = group.getAttribute('data-theme-group');
			const value = theme[key];
			group.querySelectorAll('[data-theme-value]').forEach((option) => {
				option.classList.toggle('active', option.getAttribute('data-theme-value') === value);
			});
		});

		const status = document.querySelector('[data-theme-status]');
		if (status) {
			status.textContent = `${theme.shell} shell, ${theme.background} background, ${theme.font} font, ${theme.accent} accent, ${theme.size} size`;
		}
	}

	function attachThemeControls(theme) {
		document.querySelectorAll('[data-theme-value]').forEach((button) => {
			button.addEventListener('click', () => {
				const group = button.getAttribute('data-theme-group');
				const value = button.getAttribute('data-theme-value');
				if (!group || !value) {
					return;
				}

				theme[group] = value;
				saveTheme(theme);
				applyTheme(theme);
			});
		});
	}

	const theme = loadTheme();
	applyTheme(theme);

	if (document.readyState === 'loading') {
		document.addEventListener('DOMContentLoaded', function () {
			attachThemeControls(theme);
		});
	} else {
		attachThemeControls(theme);
	}
})();
