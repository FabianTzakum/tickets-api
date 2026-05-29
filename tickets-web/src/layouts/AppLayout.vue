<script setup lang="ts">
import { computed, ref, watch } from "vue";
import { RouterLink, useRoute, useRouter } from "vue-router";
import {
  BarChart3,
  Bell,
  ChevronDown,
  ChevronRight,
  CircleHelp,
  Gauge,
  LogOut,
  Menu,
  PanelLeftClose,
  PanelLeftOpen,
  Settings,
  Ticket,
  UserRound,
  UsersRound,
  X
} from "lucide-vue-next";
import { useAuthStore } from "@/stores/authStore";

interface NavigationItem {
  label: string;
  routeName: string;
  icon: typeof Gauge;
  enabled: boolean;
  badge?: string;
}

const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();

const isSidebarCollapsed = ref(false);
const isMobileSidebarOpen = ref(false);
const isUserMenuOpen = ref(false);

const navigationItems: NavigationItem[] = [
  { label: "Dashboard", routeName: "dashboard", icon: Gauge, enabled: true },
  { label: "Tickets", routeName: "tickets", icon: Ticket, enabled: true },
  { label: "Clientes", routeName: "customers", icon: UsersRound, enabled: false, badge: "Pronto" },
  { label: "Usuarios", routeName: "users", icon: UserRound, enabled: false, badge: "Pronto" },
  { label: "Reportes", routeName: "reports", icon: BarChart3, enabled: false, badge: "Pronto" },
  { label: "Configuración", routeName: "settings", icon: Settings, enabled: false, badge: "Pronto" }
];

const activeNavigationLabel = computed(() => {
  const current = navigationItems.find((item) => item.routeName === route.name);
  return current?.label ?? "Panel";
});

const shellClasses = computed(() => ({
  "app-shell--collapsed": isSidebarCollapsed.value,
  "app-shell--mobile-open": isMobileSidebarOpen.value
}));

function getInitials(name?: string) {
  if (!name) {
    return "A";
  }

  return name
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0])
    .join("")
    .toUpperCase();
}

function closeMobileSidebar() {
  isMobileSidebarOpen.value = false;
}

function closeUserMenu() {
  isUserMenuOpen.value = false;
}

function toggleSidebar() {
  isSidebarCollapsed.value = !isSidebarCollapsed.value;
  closeUserMenu();
}

function handleDisabledClick() {
  closeMobileSidebar();
}

function handleLogout() {
  authStore.logout();
  closeUserMenu();
  closeMobileSidebar();
  router.push({ name: "login" });
}

watch(
  () => route.fullPath,
  () => {
    closeMobileSidebar();
    closeUserMenu();
  }
);
</script>

<template>
  <div class="app-shell" :class="shellClasses">
    <button
      class="mobile-menu-button"
      type="button"
      aria-label="Abrir menú principal"
      @click="isMobileSidebarOpen = true"
    >
      <Menu :size="20" />
    </button>

    <button
      v-if="isMobileSidebarOpen"
      class="app-shell__overlay"
      type="button"
      aria-label="Cerrar menú principal"
      @click="isMobileSidebarOpen = false"
    ></button>

    <aside class="app-sidebar" aria-label="Menú principal">
      <div class="app-sidebar__top">
        <RouterLink class="app-brand" :to="{ name: 'dashboard' }" @click="closeMobileSidebar">
          <span class="brand-mark" aria-hidden="true">
            <span></span>
            <span></span>
          </span>

          <div class="app-brand__text">
            <strong>Ticket App</strong>
            <small>Panel operativo</small>
          </div>
        </RouterLink>

        <button
          class="icon-button app-sidebar__collapse"
          type="button"
          :aria-label="isSidebarCollapsed ? 'Expandir menú' : 'Contraer menú'"
          @click="toggleSidebar"
        >
          <PanelLeftOpen v-if="isSidebarCollapsed" :size="18" />
          <PanelLeftClose v-else :size="18" />
        </button>

        <button
          class="icon-button app-sidebar__close"
          type="button"
          aria-label="Cerrar menú"
          @click="isMobileSidebarOpen = false"
        >
          <X :size="18" />
        </button>
      </div>

      <nav class="app-nav" aria-label="Navegación del administrador">
        <template v-for="item in navigationItems" :key="item.routeName">
          <RouterLink
            v-if="item.enabled"
            v-slot="{ href, navigate, isExactActive }"
            custom
            :to="{ name: item.routeName }"
          >
            <a
              class="app-nav__link"
              :class="{ 'app-nav__link--active': isExactActive }"
              :href="href"
              :title="item.label"
              :aria-current="isExactActive ? 'page' : undefined"
              @click="navigate"
            >
              <component :is="item.icon" :size="20" />
              <span>{{ item.label }}</span>
            </a>
          </RouterLink>

          <button
            v-else
            class="app-nav__link app-nav__link--disabled"
            type="button"
            :title="`${item.label} estará disponible pronto`"
            @click="handleDisabledClick"
          >
            <component :is="item.icon" :size="20" />
            <span>{{ item.label }}</span>
            <small>{{ item.badge }}</small>
          </button>
        </template>
      </nav>

      <div class="support-card">
        <span class="support-card__icon">
          <CircleHelp :size="20" />
        </span>

        <div class="support-card__text">
          <strong>¿Necesitas ayuda?</strong>
          <small>Soporte interno</small>
        </div>

        <ChevronRight class="support-card__arrow" :size="18" />
      </div>
    </aside>

    <main class="app-main">
      <header class="topbar">
        <div class="topbar__context">
          <p>Ticket App</p>
          <strong>{{ activeNavigationLabel }}</strong>
        </div>

        <div class="topbar__actions">
          <button class="icon-button topbar__notification" type="button" aria-label="Notificaciones">
            <Bell :size="20" />
            <span>2</span>
          </button>

          <div class="topbar__profile-wrap">
            <button
              class="topbar__profile"
              type="button"
              :aria-expanded="isUserMenuOpen"
              aria-label="Abrir menú de usuario"
              @click="isUserMenuOpen = !isUserMenuOpen"
            >
              <span class="topbar__avatar">{{ getInitials(authStore.user?.fullName) }}</span>

              <div>
                <strong>{{ authStore.user?.fullName }}</strong>
                <small>{{ authStore.userRoleLabel }}</small>
              </div>

              <ChevronDown class="topbar__chevron" :size="16" />
            </button>

            <div v-if="isUserMenuOpen" class="user-menu">
              <div class="user-menu__header">
                <strong>{{ authStore.user?.fullName }}</strong>
                <small>{{ authStore.user?.email }}</small>
              </div>

              <button type="button" @click="handleLogout">
                <LogOut :size="16" />
                Cerrar sesión
              </button>
            </div>
          </div>

          <button class="button button--ghost button--small topbar__logout" type="button" @click="handleLogout">
            <LogOut :size="16" />
            Salir
          </button>
        </div>
      </header>

      <section class="app-main__content">
        <RouterView />
      </section>
    </main>
  </div>
</template>
