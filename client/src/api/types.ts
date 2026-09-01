import type { components } from './schema'

// Thin aliases over the generated schema so the rest of the app never
// hand-writes a shape the OpenAPI spec already defines.
export type QuestionDto = components['schemas']['QuestionDto']
export type ChoiceDto = components['schemas']['ChoiceDto']
export type NextStepResponse = components['schemas']['NextStepResponse']
export type ResultResponse = components['schemas']['ResultResponse']
export type InstrumentResultDto = components['schemas']['InstrumentResultDto']
export type HelpResourceDto = components['schemas']['HelpResourceDto']
export type StartSessionResponse = components['schemas']['StartSessionResponse']
