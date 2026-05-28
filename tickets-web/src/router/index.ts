import { createRouter, createWebHistory } from "vue-router";
import { useAuthStore } from "@/stores/authStore";
import LoginView from "@/views/LoginView.vue";
import DashboardView from "@/views/DashboardView.vue";

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: "/login",
      name: "login",
      component: LoginView,
      meta: {
        public: true
      }
    },
    {
      path: "/",
      component: () => import("@/layouts/AppLayout.vue"),
      children: [
        {
          path: "",
          name: "dashboard",
          component: DashboardView
        }
      ]
    }
  ]
});

router.beforeEach(async (to) => {
  const authStore = useAuthStore();

  if (!authStore.isInitialized) {
    await authStore.initialize();
  }

  if (!to.meta.public && !authStore.isAuthenticated) {
    return {
      name: "login"
    };
  }

  if (to.name === "login" && authStore.isAuthenticated) {
    return {
      name: "dashboard"
    };
  }

  return true;
});

export default router;
