import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { CreatePersonCommand } from "./contracts";

@Injectable({
  providedIn: 'root'
})
export class StargateClient {
    private httpClient = inject(HttpClient);

    createPerson(command: CreatePersonCommand) : Observable<number> {
        return this.httpClient.post<number>('http://localhost:5001/Person', command);
    }

    createPersonRequestResponse(command: CreatePersonCommand) : Observable<number> {
        return this.httpClient.post<number>('http://localhost:5001/RequestResponsePerson', command);
    }

    createPersonAsync(command: CreatePersonCommand) : Observable<void> {
        return this.httpClient.post<void>('http://localhost:5001/AsyncPerson', command);
    }
}