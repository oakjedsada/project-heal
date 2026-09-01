import createClient from 'openapi-fetch'
import type { paths } from './schema'

// Base URL is empty: paths in the generated schema already start with /api,
// and the Vite dev server proxies /api to the backend (see vite.config.ts).
export const apiClient = createClient<paths>({ baseUrl: '' })
