export type CreatePersonCommand = {
    name: string;
}

export type PersonCreatedEvent = {
    type: 'PersonCreatedEvent',
    data: {
        id: number;
        name: string;
    };
}

export type AstronautDutyCreatedEvent = {
    type: 'AstronautDutyCreatedEvent',
    data: {
        personId: number;
        name: string;
        rank: string;
        dutyTitle: string;
        dutyStartDate: Date;
    };
}

export type ServerSentEvent = PersonCreatedEvent | AstronautDutyCreatedEvent;

export type IncomingEventResponse = {
  type: string;
  data: string;
}

export function camelCaseKeys(obj: Record<string, unknown>): Record<string, unknown> {
  const result: Record<string, unknown> = {};
  for (const key in obj) {
    const camelKey = key.charAt(0).toLowerCase() + key.slice(1);
    result[camelKey] = obj[key];
  }
  return result;
}

export function transformServerEvent(rawData: string): ServerSentEvent {
  const incoming = JSON.parse(rawData) as IncomingEventResponse;
  const eventType = incoming.type;
  
  return {
    type: eventType as any,
    data: camelCaseKeys(JSON.parse(incoming.data)) as any
  };
}

export function createServerEvent(incomingEvent: IncomingEventResponse): ServerSentEvent {  
  return {
    type: incomingEvent.type as any,
    data: camelCaseKeys(JSON.parse(incomingEvent.data)) as any
  };
}