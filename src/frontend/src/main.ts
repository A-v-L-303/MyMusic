import { bootstrapApplication } from '@angular/platform-browser';
import { App } from './app/app';
import { buildAppConfig } from './app/app.config';
import { RuntimeConfig } from './app/core/runtime-config/runtime-config.service';

async function loadRuntimeConfig(): Promise<RuntimeConfig> {
  const response = await fetch('/runtime-config.json');

  if (!response.ok) {
    throw new Error(`runtime-config.json konnte nicht geladen werden (Status ${response.status}).`);
  }

  return (await response.json()) as RuntimeConfig;
}

loadRuntimeConfig()
  .then((runtimeConfig) => bootstrapApplication(App, buildAppConfig(runtimeConfig)))
  .catch((err) => console.error(err));
