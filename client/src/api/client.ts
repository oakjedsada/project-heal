import createClient from 'openapi-fetch'
import type { paths } from './schema'

// Empty by default: paths in the generated schema already start with /api,
// and the Vite dev server proxies /api to the backend (see vite.config.ts),
// so local dev never needs an absolute URL. A real deploy where the client
// and API are separate services (e.g. two Railway services) sets
// VITE_API_BASE_URL at build time to the API's public URL instead.
const baseUrl = import.meta.env.VITE_API_BASE_URL ?? ''

export const apiClient = createClient<paths>({ baseUrl })
