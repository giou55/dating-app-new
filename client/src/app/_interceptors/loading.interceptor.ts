import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { delay, finalize, identity, Observable } from 'rxjs';
import { BusyService } from '../_services/busy.service';
import { environment } from 'src/environments/environment';

// interceptors handles and transform the outgoing request
// or the response

@Injectable()
export class LoadingInterceptor implements HttpInterceptor {

  constructor(private busyService: BusyService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    // once some request has started, turn on the spinner
    this.busyService.busy();

    return next.handle(request).pipe(
      // we add a delay so we can see the spinner,
      // but only in development mode
      (environment.production ? identity : delay(1000)), // identity is a operator that's does nothing
      // once the request has completed, turn off the spinner
      finalize(() => {
        this.busyService.idle();
      })
    )
  }
}
