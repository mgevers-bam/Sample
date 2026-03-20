import { HttpClient } from '@angular/common/http';
import { Injectable, NgZone, OnDestroy, inject } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { ServerSentEvent, transformServerEvent } from './contracts';

@Injectable({
  providedIn: 'root'
})
export class StargateServerSentEventsService implements OnDestroy {
  private eventSource: EventSource | null = null;
  private eventSubject = new Subject<ServerSentEvent>();
  private errorSubject = new Subject<Event>();
  private connectionStateSubject = new Subject<boolean>();

  private ngZone = inject(NgZone);

  connect(baseUrl: string): Observable<ServerSentEvent> {
    if (this.eventSource) {
      return this.eventSubject.asObservable();
    }

    const url = `${baseUrl}/api/ServerSentEvents/stream`;

    this.ngZone.runOutsideAngular(() => {
      this.eventSource = new EventSource(url);

      this.eventSource.onopen = () => {
        this.ngZone.run(() => {
          this.connectionStateSubject.next(true);
        });
      };

      this.eventSource.onmessage = (event: MessageEvent<string>) => {
        this.ngZone.run(() => {
          const serverEvent = transformServerEvent(event.data);
          this.eventSubject.next(serverEvent);
        });
      };

      this.eventSource.onerror = (error: Event) => {
        this.ngZone.run(() => {
          this.errorSubject.next(error);
          this.connectionStateSubject.next(false);
        });
      };
    });

    return this.eventSubject.asObservable();
  }

  disconnect(): void {
    if (this.eventSource) {
      this.eventSource.close();
      this.eventSource = null;
      this.connectionStateSubject.next(false);
    }
  }

  ngOnDestroy(): void {
    this.disconnect();
    this.eventSubject.complete();
    this.errorSubject.complete();
    this.connectionStateSubject.complete();
  }
}