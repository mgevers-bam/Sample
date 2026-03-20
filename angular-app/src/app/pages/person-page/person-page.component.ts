import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { lastValueFrom, Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatRadioModule } from '@angular/material/radio';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { StargateServerSentEventsService } from '../../features/contacts/services/stargate-client.service-sent-events.service';
import { ServerSentEvent } from '../../features/contacts/services/contracts';
import { StargateSignalRService } from '../../features/contacts/services/stargate-client.signalR.service';
import { StargateClient } from '../../features/contacts/services/stargate-client.service';

@Component({
  selector: 'app-person-page',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatRadioModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './person-page.component.html',
  styleUrls: ['./person-page.component.scss'],
})
export class PersonPageComponent implements OnInit {
  private readonly serverSentEventsService = inject(StargateServerSentEventsService);
  private readonly signalRService = inject(StargateSignalRService);
  private readonly stargateClient = inject(StargateClient);
  private readonly fb = inject(FormBuilder);

  serverSentEvent$: Observable<ServerSentEvent> | null = null;
  signalREvent$: Observable<ServerSentEvent> | null = null;
  createPersonForm!: FormGroup;
  isSubmitting = false;

  transportMethods = ['mediator', 'requestResponse', 'async', 'socket'];

  ngOnInit(): void {
    this.initializeForm();
    this.signalRService.connect('http://localhost:5001');

    this.serverSentEvent$ = this.serverSentEventsService.connect('http://localhost:5001').pipe(
      tap(event => {
        switch (event.type) {
          case 'PersonCreatedEvent':
            console.log('PersonCreated Server Sent Event received:', event.data);
            break;
          case 'AstronautDutyCreatedEvent':
            console.log('AstronautDutyCreated Server Sent Event received:', event.data);
            break;
          default:
            console.warn('UNKNOWN event type received:', event);
        }
      })
    );

    this.signalREvent$ = this.signalRService.serverEvents$.pipe(
      tap(event => {
        switch (event.type) {
          case 'PersonCreatedEvent':
            console.log('PersonCreated SignalR Event received:', event.data);
            break;
          case 'AstronautDutyCreatedEvent':
            console.log('AstronautDutyCreated SignalR Event received:', event.data);
            break;
          default:
            console.warn('UNKNOWN event type received:', event);
        }
      })
    );
  }

  private initializeForm(): void {
    this.createPersonForm = this.fb.group({
      name: ['', Validators.required],
      transportMethod: ['mediator', Validators.required]
    });
  }

  async onSubmit(): Promise<void> {
    if (this.createPersonForm.valid) {
      this.isSubmitting = true;
      const formValue = this.createPersonForm.value;
      const obs: Observable<number | void> = this.getSubmitObservable(formValue.transportMethod, formValue.name).pipe(
        tap(() => {
          console.info('reset');
          this.createPersonForm.reset({ transportMethod: 'mediator' });
        })
      );

      try {
        await lastValueFrom(obs);
      } finally {
        this.isSubmitting = false;
      }
    }
  }

  private getSubmitObservable(transportMethod: string, name: string): Observable<number | void> {
    switch (transportMethod) {
        case 'mediator':
          return this.stargateClient.createPerson({ name });
        case 'requestResponse':
          return this.stargateClient.createPersonRequestResponse({ name });
        case 'async':
          return this.stargateClient.createPersonAsync({ name });
        case 'socket':
          return this.signalRService.createPerson({ name });
        default:
          throw new Error(`Unknown transport method: ${transportMethod}`);
      }
  }
}
