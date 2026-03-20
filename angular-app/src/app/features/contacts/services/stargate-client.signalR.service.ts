import { Injectable, OnDestroy } from '@angular/core';
import { Observable, Subject, from } from 'rxjs';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { ServerSentEvent,  CreatePersonCommand, IncomingEventResponse, createServerEvent } from './contracts';

@Injectable({
    providedIn: 'root'
})
export class StargateSignalRService implements OnDestroy {
    private hub: HubConnection | null = null;
    private eventSubject = new Subject<ServerSentEvent>();
    private errorSubject = new Subject<Error>();
    private connectionStateSubject = new Subject<boolean>();

    serverEvents$ = this.eventSubject.asObservable();

    async connect(baseUrl: string): Promise<void> {
        if (this.hub?.state === HubConnectionState.Connected) {
            return;
        }

        const hubUrl = `${baseUrl}/api/server-events-hub`;
        const token = 'eyJhbGciOiJIUzI1NiIsImtpZCI6ImY0OWU2NDE2LWI5MzMtNGMyOC05ZGI3LTQwM2E0YzFjNjZmNyIsInR5cCI6IkpXVCJ9.eyJQZXJzb25JZGVudGlmaWVyIjoiYjNiZGM3YjQtMDFhYS00MTFjLWE0NzAtNzc0MzY0YzUzODEzIiwiZXhwIjoxNzc1MDYzNzI4LCJpc3MiOiIwNDg1NTViMy1hMDYxLTQzZTUtODZlMy0xMDk0ZmUwZTZhYzMifQ.x_pTzwm6zfss3tQDuVSFz3siIyd2itSp6BtLS69BVIs'

        this.hub = new HubConnectionBuilder()
            .withUrl(hubUrl, {
                withCredentials: true,      
                accessTokenFactory: () => token,
                // Use automatic reconnection
                skipNegotiation: false,
                transport: 0 // Automatic
            })
            .withAutomaticReconnect([0, 2000, 10000]) // Retry delays
            .build();

        this.hub.on('PushServerEvent', (serverEvent: IncomingEventResponse) => {
            try {
                this.eventSubject.next(createServerEvent(serverEvent));
            } catch (error) {
                this.errorSubject.next(error as Error);
            }
        });

        this.hub.onclose = (error?) => {
            if (error) {
                const err = error instanceof Error ? error : new Error(String(error));
                this.errorSubject.next(err);
            }
            this.connectionStateSubject.next(false);
        };

        await this.hub.start()
            .then(() => {
                this.connectionStateSubject.next(true);
            }).catch(err => {
                this.errorSubject.next(err);
                this.connectionStateSubject.next(false);
            });
    }

    disconnect(): void {
        if (this.hub) {
            this.hub.stop().then(() => {
                this.connectionStateSubject.next(false);
            }).catch(err => {
                console.error('Error stopping hub:', err);
            });
            this.hub = null;
        }
    }

    createPerson(command: CreatePersonCommand): Observable<void> {
        if (!this.hub) {
            return new Observable(observer => {
                observer.error(new Error('Hub not connected'));
            });
        }

        return from(this.hub.invoke<void>('CreatePerson', command));
    }

    ngOnDestroy(): void {
        this.disconnect();
        this.eventSubject.complete();
        this.errorSubject.complete();
        this.connectionStateSubject.complete();
    }
}