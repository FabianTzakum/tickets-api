<script setup lang="ts">
import { reactive, ref } from "vue";
import { useRouter } from "vue-router";
import { Eye, EyeOff, LockKeyhole, Mail, ShieldCheck } from "lucide-vue-next";
import { useAuthStore } from "@/stores/authStore";

const router = useRouter();
const authStore = useAuthStore();
const showPassword = ref(false);

const form = reactive({
  email: "admin@tickets.local",
  password: "Admin12345"
});

async function handleSubmit() {
  const success = await authStore.login({
    email: form.email,
    password: form.password
  });

  if (success) {
    router.push({ name: "dashboard" });
  }
}
</script>

<template>
  <main class="login-page">
    <section class="login-shell">
      <aside class="login-shell__visual">
        <div class="login-shell__glow"></div>

        <div class="login-shell__symbol">
          <span></span>
          <span></span>
        </div>

        <div class="login-shell__copy">
          <h1>Ticket App</h1>
          <p>Soporte. Tickets. Soluciones.</p>
        </div>

        <div class="login-shell__wave"></div>
      </aside>

      <form class="login-form" @submit.prevent="handleSubmit">
        <div class="login-form__header">
          <p class="section-label">
            <ShieldCheck :size="16" />
            Acceso seguro
          </p>
          <h2>Iniciar sesión</h2>
        </div>

        <label class="form-field">
          <span>Correo</span>
          <div class="input-control">
            <Mail class="input-control__icon" :size="20" />
            <input v-model="form.email" type="email" autocomplete="email" placeholder="ejemplo@correo.com" />
          </div>
        </label>

        <label class="form-field">
          <span>Contraseña</span>
          <div class="input-control">
            <LockKeyhole class="input-control__icon" :size="20" />
            <input
              v-model="form.password"
              :type="showPassword ? 'text' : 'password'"
              autocomplete="current-password"
              placeholder="••••••••"
            />
            <button class="input-control__action" type="button" @click="showPassword = !showPassword">
              <EyeOff v-if="showPassword" :size="19" />
              <Eye v-else :size="19" />
            </button>
          </div>
        </label>

        <a class="login-form__link" href="#" aria-label="Recuperar contraseña">
          ¿Olvidaste tu contraseña?
        </a>

        <p v-if="authStore.errorMessage" class="form-error">
          {{ authStore.errorMessage }}
        </p>

        <button class="button button--primary button--block" type="submit" :disabled="authStore.isLoading">
          {{ authStore.isLoading ? "Validando..." : "Entrar" }}
        </button>

        <div class="login-form__divider">
          <span></span>
          <strong>o</strong>
          <span></span>
        </div>

        <p class="login-form__support">
          ¿Necesitas ayuda? Contacta a <strong>soporte</strong>.
        </p>
      </form>
    </section>
  </main>
</template>
