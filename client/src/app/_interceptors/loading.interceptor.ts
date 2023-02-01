import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { delay, finalize, Observable } from 'rxjs';
import { BusyService } from '../_services/busy.service';

// interceptors handles and transform the outgoing request
// or the response

@Injectable()
export class LoadingInterceptor implements HttpInterceptor {

  constructor(private busyService: BusyService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    // once some request has started, turn on the spinner
    this.busyService.busy();

    return next.handle(request).pipe(
      // we add a delay so we can see the spinner
      delay(1000),
      // once the request has completed, turn off the spinner
      finalize(() => {
        this.busyService.idle();
      })
    )
  }
}
